using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using wmsMLC.General.BL;

namespace wmsMLC.General.PL.Model
{
    /// <summary> Класс, описывающий поля сущности </summary>
    public class DataField : ICloneable
    {
        public DataField()
        {
            // задаем default-ные параметры
            Visible = true;
            IsChangeLookupCode = false;
            //IsEnabled = true;
            LookupButtonEnabled = true;
            Properties = new Dictionary<string, object>();
        }

        /// <summary> Уникальное имя поля </summary>
        public string Name { get; set; }

        /// <summary> Имя поля класса, к которому осуществляется привязка. </summary>
        /// <remarks>Внимание: данное имя не уникальное, т.к. должна иметься возможность привязывать разные поля к одному полю в классе</remarks>
        public string FieldName { get; set; }

        /// <summary> Имя поля в источнике данных. </summary>
        /// <remarks> Обычно данное поле используется для построения фильтров</remarks>
        public string SourceName { get; set; }

        /// <summary> Наименование </summary>
        public string Caption { get; set; }

        /// <summary> Заливка </summary>
        public string BackGroundColor { get; set; }

        /// <summary> Описание поля </summary>
        public string Description { get; set; }

        /// <summary> Тип данных </summary>
        public Type FieldType { get; set; }

        /// <summary> Признак того, что поле доступно для редактирования </summary>
        public bool? IsEnabled { get; set; }

        /// <summary> Признак того, что поле доступно для отображения </summary>
        public bool Visible { get; set; }

        /// <summary> Формат отображения данных </summary>
        public string DisplayFormat { get; set; }

        public string LookupCode { get; set; }

        /// <summary>
        /// Дополнительный фильтр заданный извне
        /// </summary>
        public string LookupFilterExt { get; set; }

        /// <summary>
        /// Дополнительный var фильтр заданный извне
        /// </summary>
        public string LookupVarFilterExt { get; set; }

        /// <summary>
        /// Показывать ли у lookup кнопку добавить 
        /// </summary>
        public bool LookupButtonEnabled { get; set; }

        /// <summary>
        /// Дополнительный признак, что field может менять свои признаки для View
        /// </summary>
        public bool IsChangeLookupCode { get; set; }

        public string KeyLink { get; set; }

        public bool AllowAddNewValue { get; set; }

        /// <summary>
        /// Максимальная длина поля
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Доступность поля при создании объекта
        /// </summary>
        public bool? EnableCreate { get; set; }

        /// <summary>
        /// Доступность поля при изменении объекта
        /// </summary>
        public bool? EnableEdit { get; set; }

        /// <summary>
        /// Признак того, отображать ли значения через спец. поле
        /// </summary>
        public bool IsMemoView { get; set; }
        
        /// <summary>
        /// Path для создания Binding.
        /// </summary>
        public string BindingPath { get; set; }

        /// <summary>
        /// Конвертор для отображения.
        /// </summary>
        public IValueConverter DisplayTextConverter { get; set; }

        /// <summary>
        /// Дополнительные свойства.
        /// </summary>
        public IDictionary<string, object> Properties { get; protected set; }

        public T Get<T>(string name)
        {
            if (string.IsNullOrEmpty(name) || !Properties.ContainsKey(name))
                return default(T);

            try
            {
                return (T)SerializationHelper.ConvertToTrueType(Properties[name], typeof(T));
            }
            catch (Exception ex)
            {
                throw new OperationException(Resources.ExceptionResources.TypeConversionError, "ValueDataField.", name, typeof(T), ex);
            }
        }

        public void Set<T>(string name, T value)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            Properties[name] = value;
        }

        public static bool TryGetFieldProperties<T>(DataField field, string name, bool validateOnWfDesignMode, out T value)
        {
            value = default(T);

            if (field.Properties.ContainsKey(name))
            {
                if (validateOnWfDesignMode && ValueDataFieldConstants.ValidateUseWfVariable(field.Properties[name]))
                    return false;
                try
                {
                    value = field.Get<T>(name);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        protected virtual object Clone(object source)
        {
            if (source == null)
                return null;

            var properties = TypeDescriptor.GetProperties(this).Cast<PropertyDescriptor>();
            foreach (var property in properties)
            {
                var value = property.GetValue(this);
                if (value != null)
                    property.SetValue(source, value);
            }

            var datafield = source as DataField;
            if (datafield != null)
                datafield.Properties = new Dictionary<string, object>(Properties);
            return source;
        }

        public virtual object Clone()
        {
            return Clone(new DataField());
        }
    }
}
