using System.Activities;
using System.Activities.Validation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using wmsMLC.Activities.General;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Components.Controls.Rcl;
using wmsMLC.General.PL.WPF.Components.ViewModels;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.Activities.RclViewInteraction
{
    public class RclMustCompareActivity<T> : NativeActivity
    {
        private const string EntitiesDisplayName = @"Объекты валидации";
        private const string ArtCodePropertyNameDisplayName = @"Наименование свойства кода артикула для данного типа";
        private const string OperationCodeDisplayName = @"Код операции";
        
        private PMMethods[] _pmMethods;

        public RclMustCompareActivity()
        {
            DisplayName = "ТСД: Применение методов 'MUST_COMPARE' к массиву сущностей";
            FontSize = 14;
            MaxRowsOnPage = CustomSelectControlBase.MaxRows;
            _pmMethods = new[] {PMMethods.MUST_COMPARE, PMMethods.MUST_COMPARE_VISIBLE};
        }

        #region .  Properties  .
        [DisplayName(@"Размер шрифта")]
        [DefaultValue(14)]
        public InArgument<double> FontSize { get; set; }

        [DisplayName(EntitiesDisplayName)]
        public InArgument<T[]> Entities { get; set; }

        [DisplayName(ArtCodePropertyNameDisplayName)]
        public InArgument<string> ArtCodePropertyName { get; set; }

        [DisplayName(OperationCodeDisplayName)]
        public InArgument<string> OperationCode { get; set; }

        [DisplayName(@"Максимальное количество строк в Lookup'е")]
        public InArgument<int> MaxRowsOnPage { get; set; }

        [DisplayName(@"Результат")]
        public OutArgument<T[]> Result { get; set; }
        #endregion .  Properties  .

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, FontSize, type.ExtractPropertyName(() => FontSize));
            ActivityHelpers.AddCacheMetadata(collection, metadata, ArtCodePropertyName, type.ExtractPropertyName(() => ArtCodePropertyName));
            ActivityHelpers.AddCacheMetadata(collection, metadata, OperationCode, type.ExtractPropertyName(() => OperationCode));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Entities, type.ExtractPropertyName(() => Entities));
            ActivityHelpers.AddCacheMetadata(collection, metadata, MaxRowsOnPage, type.ExtractPropertyName(() => MaxRowsOnPage));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Result, type.ExtractPropertyName(() => Result));

            metadata.SetArgumentsCollection(collection);

            var typeT = typeof (T);
            if (!typeof (WMSBusinessObject).IsAssignableFrom(typeT))
                metadata.AddValidationError(new ValidationError(string.Format("Тип '{0}' не поддерживается.", typeT)));
        }

        #region .  Methods  .

        protected override void Execute(NativeActivityContext context)
        {
            const string isnullerrorformat = "Свойство '{0}' должно быть задано.";

            var entities = Entities.Get(context);
            if (entities == null)
                throw new DeveloperException(isnullerrorformat, EntitiesDisplayName);

            var artcodepropertyname = ArtCodePropertyName.Get(context);
            if (string.IsNullOrEmpty(artcodepropertyname))
                throw new DeveloperException(isnullerrorformat, ArtCodePropertyNameDisplayName);

            var operationCode = OperationCode.Get(context);
            if (string.IsNullOrEmpty(operationCode))
                throw new DeveloperException(isnullerrorformat, OperationCodeDisplayName);

            var boEntities = entities.OfType<WMSBusinessObject>().ToArray();

            //Получаем список артикулов из Entities
            var arts = boEntities.Select(p => p.GetProperty<string>(artcodepropertyname)).Distinct().ToArray();

            var result = boEntities.ToArray();

            var objFields = DataFieldHelper.Instance.GetDataFields(typeof(T), SettingDisplay.Detail);

            foreach (var artcode in arts)
            {
                var propList = GetPmMethodProperies(artcode, operationCode, context);
                var boEntity = result.FirstOrDefault(p => p.GetProperty<string>(artcodepropertyname) == artcode);
                if (boEntity == null)
                    continue;

                foreach (var method in propList.Keys)
                {
                    foreach (var property in propList[method])
                    {
                        var compareValues = result.Select(p => p.GetProperty(property)).Distinct().ToArray();
                        if (compareValues.Length <= 1)
                            continue; //Пропускаем

                        var datafield = objFields.FirstOrDefault(i => i.Name.EqIgnoreCase(property));
                        if (datafield == null)
                            throw new DeveloperException(
                                "Ошибка в настройках '{0}' PM. Задан неизвестный параметр '{1}'.", method, property);

                        var field = new ValueDataField(datafield) {LabelPosition = "Top"};
                        if (method == PMMethods.MUST_COMPARE_VISIBLE)
                            field.Value = boEntity.GetProperty(property);

                        if (string.IsNullOrEmpty(field.LookupCode))
                        {
                            field.CloseDialog = true;
                        }
                        else //Если поле лукап
                        {
                            field.LabelPosition = "None";
                            field.Set(ValueDataFieldConstants.LookupType, RclLookupType.SelectControl.ToString());
                            field.Set(ValueDataFieldConstants.MaxRowsOnPage, MaxRowsOnPage.Get(context));
                        }

                        var model = CreateDialogModel(context, field);
                        while (true) // делаем это для возврата на форму
                        {
                            if (!ShowDialog(model))
                                throw new OperationException("Значения полей не были указаны.");

                            var errorMessage = new StringBuilder();
                            switch (string.Format("1{0}", model.MenuResult))
                            {
                                case "1":
                                case "1Return":
                                    var value = model[field.Name];
                                    if (!compareValues.Any(p => Equals(value, p)))
                                    {
                                        errorMessage.AppendLine(string.Format("Неверное значение поля '{0}'.",
                                            field.Caption));
                                    }
                                    else
                                    {
                                        result = result.Where(p => Equals(value, p.GetProperty(property))).ToArray();
                                    }
                                    break;
                                default:
                                    throw new DeveloperException("Неизвестная команда.");
                            }
                            // если были ошибки, то покажем что не так и вернемся на форму
                            if (errorMessage.Length > 0)
                            {
                                var viewService = IoC.Instance.Resolve<IViewService>();
                                viewService.ShowDialog("Ошибка", errorMessage.ToString(), MessageBoxButton.OK,
                                    MessageBoxImage.Error, MessageBoxResult.OK);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }

            Result.Set(context, result.OfType<T>().ToArray());
        }

        private IDictionary<PMMethods, string[]> GetPmMethodProperies(string artCode, string operationCode, NativeActivityContext context)
        {
            var result = new Dictionary<PMMethods, string[]>();
            var entityname = SourceNameHelper.Instance.GetSourceName(typeof (T)).ToUpper();

            using (var pmConfigMgr = IoC.Instance.Resolve<IPMConfigManager>())
            {
                SetUnitOfWork(context, pmConfigMgr);
                foreach (var method in _pmMethods)
                {
                    var pmconfigs = pmConfigMgr.GetPMConfigByParamListByArtCode(artCode, operationCode, method.ToString()).ToArray();
                    if (pmconfigs.Length > 0)
                    {
                        var props =
                            pmconfigs.Where(p => p.ObjectEntitycode_R.ToUpper() == entityname)
                                .Select(p => p.ObjectName_r)
                                .ToArray();
                        if (props.Length > 0)
                            result[method] = props;
                    }
                }
            }

            return result;
        }

        private void SetUnitOfWork(NativeActivityContext context, IBaseManager mgr)
        {
            var uw = BeginTransactionActivity.GetUnitOfWork(context);
            if (uw != null)
                mgr.SetUnitOfWork(uw);
        }

        private DialogSourceViewModel CreateDialogModel(ActivityContext context, ValueDataField property)
        {
            var model = new DialogSourceViewModel
            {
                PanelCaption = "Введите значение",
                FontSize = FontSize.Get(context),
                IsMenuVisible = false,
            };

            // добавим параметр
            property.SetFocus = true;
            model.Fields.Add(property);

            var footerMenu = new List<ValueDataField>();

            var footerMenuItem = new ValueDataField
            {
                Name = "Menu0",
                Caption = "Назад",
                Value = "Escape"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 0);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 0);
            footerMenu.Add(footerMenuItem);

            footerMenuItem = new ValueDataField
            {
                Name = "Menu1",
                Caption = "Далее",
                Value = "Enter"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 0);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 1);
            footerMenu.Add(footerMenuItem);

            var footerMenufield = new ValueDataField
            {
                Name = ValueDataFieldConstants.FooterMenu,
                Caption = ValueDataFieldConstants.FooterMenu,
                FieldType = typeof(FooterMenu),
                IsEnabled = true
            };
            footerMenufield.Set(ValueDataFieldConstants.FooterMenu, footerMenu.ToArray());
            model.Fields.Add(footerMenufield);

            model.UpdateSource();
            return model;
        }

        private bool ShowDialog(IViewModel model)
        {
            var viewService = IoC.Instance.Resolve<IViewService>();
            return viewService.ShowDialogWindow(model, false) == true;
        }
        #endregion .  Methods  .
    }
}
