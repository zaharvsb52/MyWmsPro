using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using BLToolkit.Aspects;
using BLToolkit.Reflection;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Validation.Attributes;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Attributes;
using wmsMLC.General.PL.WPF.Converters;

namespace wmsMLC.General.PL.WPF.Helpers
{
    public abstract class DataFieldHelper : ICacheable
    {
        #region .  Singleton realization  .
        private static readonly Lazy<DataFieldHelper> _instance = new Lazy<DataFieldHelper>(TypeAccessor.CreateInstance<DataFieldHelper>);
        public static DataFieldHelper Instance
        {
            get { return _instance.Value; }
        }
        #endregion

        /// <summary>
        /// Получить поля объекта
        /// </summary>
        /// <param name="type">Тип объекта. Применяется для поиска доступных св-в</param>
        /// <param name="setDisplay">Тип набора, который необходимо получить</param>
        /// <remarks>ВНИМАНИЕ: данные кэшируются! Нельзя изменять данные после получения. Если вы хотите коллекцию, которую потом будете изменять - используйте GetNewDataFields</remarks>
        /// <returns>Коллекция доступных св-в данного типа для требуемого отображения</returns>
        [Cache]
        protected virtual IEnumerable<DataField> GetDataFieldsInternal(Type type, SettingDisplay setDisplay)
        {
            return CreateDataFields(type, setDisplay);
        }

        public virtual ObservableCollection<DataField> GetDataFields(Type type, SettingDisplay setDisplay)
        {
            var res = GetDataFieldsInternal(type, setDisplay);
            var clonedFields = res.Select(i => i.Clone()).Cast<DataField>().ToList();
            return new ObservableCollection<DataField>(clonedFields);
        }

        private IEnumerable<DataField> CreateDataFields(Type type, SettingDisplay setDisplay)
        {
            var cln = new ObservableCollection<DataField>();

            PropertyDescriptorCollection properties;
            if (typeof(ICustomTypeDescriptor).IsAssignableFrom(type))
            {
                var model = Activator.CreateInstance(type);
                properties = ((ICustomTypeDescriptor)model).GetProperties();
            }
            else
            {
                properties = TypeDescriptor.GetProperties(type);
            }

            foreach (PropertyDescriptor p in properties)
            {
                var df = new DataField { Caption = p.DisplayName };

                if (!string.IsNullOrEmpty(p.Description))
                    df.Description = p.Description;

                df.FieldType = p.PropertyType;

                if (typeof(BusinessObject).IsAssignableFrom(df.FieldType))
                {
                    var items = Instance.GetDataFields(df.FieldType, setDisplay);

                    foreach (var dataField in items.Select(dataField => (DataField)dataField.Clone()))
                    {
                        dataField.Caption = string.Format("{0}: {1}", p.DisplayName, dataField.Caption);
                        dataField.FieldName = string.Format("{0}.{1}", p.Name, dataField.FieldName);
                        cln.Add(dataField);
                    }
                    continue;
                }

                df.SourceName = SourceNameHelper.Instance.GetPropertySourceName(type, p.Name);
                df.Name = p.Name;
                df.FieldName = p.Name;
                // если параметр только на чтение, то выставим
                if (p.IsReadOnly)
                    df.IsEnabled = false;
                df.EnableCreate = EnableCreateAttribute.DefaultEnableCreate;
                df.EnableEdit = EnableEditAttribute.DefaultEnableEdit;

                BaseVisibleAttribute visibleAttribute = null;
                BaseFormatAttribute displayFormatAttribute = null;

                switch (setDisplay)
                {
                    case SettingDisplay.List:
                    case SettingDisplay.Tree:
                        visibleAttribute = p.Attributes[typeof(ListVisibleAttribute)] as BaseVisibleAttribute;
                        displayFormatAttribute = p.Attributes[typeof(ListDisplayFormatAttribute)] as BaseFormatAttribute
                            ?? p.Attributes[typeof(DefaultDisplayFormatAttribute)] as BaseFormatAttribute;
                        df.IsMemoView = p.Attributes[typeof(ListMemoVisibleAttribute)] as ListMemoVisibleAttribute != null;
                        break;

                    case SettingDisplay.SubList:
                        visibleAttribute = p.Attributes[typeof(SubListVisibleAttribute)] as BaseVisibleAttribute;
                        displayFormatAttribute = p.Attributes[typeof(LookUpDisplayFormatAttribute)] as BaseFormatAttribute
                            ?? p.Attributes[typeof(DefaultDisplayFormatAttribute)] as BaseFormatAttribute;
                        break;

                    case SettingDisplay.SubTree:
                        visibleAttribute = p.Attributes[typeof(SubListVisibleAttribute)] as BaseVisibleAttribute;
                        displayFormatAttribute = p.Attributes[typeof(DefaultDisplayFormatAttribute)] as BaseFormatAttribute;
                        break;

                    case SettingDisplay.Detail:
                        visibleAttribute = p.Attributes[typeof(DetailVisibleAttribute)] as BaseVisibleAttribute;
                        displayFormatAttribute = p.Attributes[typeof(DetailDisplayFormatAttribute)] as BaseFormatAttribute
                            ?? p.Attributes[typeof(DefaultDisplayFormatAttribute)] as BaseFormatAttribute;
                        df.IsMemoView = p.Attributes[typeof(DetailMemoVisibleAttribute)] as DetailMemoVisibleAttribute != null;
                        break;

                    case SettingDisplay.SubDetail:
                        visibleAttribute = p.Attributes[typeof(SubDetailVisibleAttribute)] as BaseVisibleAttribute;
                        displayFormatAttribute = p.Attributes[typeof(DefaultDisplayFormatAttribute)] as BaseFormatAttribute;
                        break;

                    case SettingDisplay.LookUp:
                        visibleAttribute = p.Attributes[typeof(LookUpVisibleAttribute)] as BaseVisibleAttribute;
                        displayFormatAttribute = p.Attributes[typeof(LookUpDisplayFormatAttribute)] as BaseFormatAttribute
                            ?? p.Attributes[typeof(DefaultDisplayFormatAttribute)] as BaseFormatAttribute;
                        break;

                    case SettingDisplay.Filter:
                        visibleAttribute = p.Attributes[typeof(FilterVisibleAttribute)] as BaseVisibleAttribute;
                        // Применяем ListDisplayFormatAttribute, т.к. не устанавливаем отдельный формат для фильтра
                        displayFormatAttribute = p.Attributes[typeof(ListDisplayFormatAttribute)] as BaseFormatAttribute
                            ?? p.Attributes[typeof(DefaultDisplayFormatAttribute)] as BaseFormatAttribute;
                        break;

                    case SettingDisplay.All:
                        break;

                    default:
                        throw new DeveloperException("SettingDisplay={0}.", setDisplay);
                }

                if (setDisplay != SettingDisplay.All &&
                    visibleAttribute != null && !visibleAttribute.Visible)
                    continue;

                // можно ли отображать
                //                if (visibleAttribute != null)
                //                    df.Visible = visibleAttribute.Visible;

                // формат отображения
                if (displayFormatAttribute != null)
                {
                    var str = displayFormatAttribute.DisplayFormat;
                    // конвертор
                    if (str.ToUpper().StartsWith("CONVERTOR:"))
                    {
                        var ar = str.Split(':');
                        if (ar != null && ar.Length > 2)
                        {
                            df.Set(ValueDataFieldConstants.BindingIValueConverter, new ConverterSI() { ParamFrom = ar[1], ParamTo = ar[2], Mode = ModeConverter.Converter});
                            switch (setDisplay)
                            {
                                case SettingDisplay.Detail:
                                case SettingDisplay.SubDetail:
                                    df.DisplayTextConverter = new ConverterSI() { ParamTo = ar[2], ParamFrom = ar[1], Mode = ModeConverter.Format };
                                    break;
                                default:
                                    df.DisplayTextConverter = new ConverterSI() { ParamTo = ar[2], ParamFrom = ar[1], Mode = ModeConverter.FormatAndConverter };
                                    break;
                            }
                        }
                    }
                    else
                        df.DisplayFormat = str;
                }
                else
                {
                    //если формата не задали, то для DataTime всегда будет длинный формат
                    if (df.FieldType.GetNonNullableType() == typeof(DateTime))
                        df.DisplayFormat = "dd.MM.yyyy HH:mm:ss";
                }


                var attrLook = p.Attributes[typeof(LookupAttribute)] as LookupAttribute;
                if (attrLook != null)
                {
                    df.LookupCode = attrLook.LookUp;
                    df.KeyLink = attrLook.KeyLink;
                    if (!string.IsNullOrEmpty(attrLook.KeyLink))
                        df.FieldName = attrLook.KeyLink;
                }

                var maxLengthAttribute = p.Attributes[typeof(MaxLengthAttribute)] as MaxLengthAttribute;
                if (maxLengthAttribute != null)
                    // +1 добавлен, чтобы сработала валидация при наступлении maxLength
                    df.MaxLength = maxLengthAttribute.MaxLength + 1;

                var enableCreateAttibute = p.Attributes[typeof(EnableCreateAttribute)] as EnableCreateAttribute;
                if (enableCreateAttibute != null)
                    df.EnableCreate = enableCreateAttibute.Enable;

                var enableEditAttribute = p.Attributes[typeof(EnableEditAttribute)] as EnableEditAttribute;
                if (enableEditAttribute != null)
                    df.EnableEdit = enableEditAttribute.Enable;

                cln.Add(df);
            }

            return cln;
        }

        [ClearCache]
        public abstract void ClearCache();
    }
}
