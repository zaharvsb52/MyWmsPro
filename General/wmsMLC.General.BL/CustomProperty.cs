using System;

namespace wmsMLC.General.BL
{
    public class CustomProperty
    {
        #region .  Constants & Variables  .
        public static Type DefaultType = typeof(string);
        public static object DefaultDefaultValue = null;

        private object _value;
        #endregion

        #region .  Constructors  .
        public CustomProperty(string name) : this(name, DefaultType, DefaultDefaultValue) { }

        public CustomProperty(string name, Type type, object defaultValue)
        {
            Name = name;

            // nullable тип ни на что менять не нужно
            if (type.IsNullable() && defaultValue == null)
            {
                PropertyType = type;
            }
            else
            {
                // если от нас хотят value type (ex. int), а дефолт == null, то от нас хотят Nullable
                if (type.IsValueType && defaultValue == null)
                    PropertyType = typeof (Nullable<>).MakeGenericType(type);
                else
                    PropertyType = type;
            }

            // оперделяем и выставляем default
            DefaultValue = SerializationHelper.ConvertToTrueType(defaultValue, PropertyType);
            SetToDefault();
        }
        #endregion

        #region .  Properties  .

        public string Name { get; private set; }

        public Type PropertyType { get; private set; }

        internal object Value
        {
            get { return GetValue(); }
            set
            {
                SetValue(value);
            }
        }

        internal object DefaultValue { get; set; }

        #endregion

        #region .  Methods  .
        internal void SetNewInstance()
        {
            // если есть default - то его и выставляем
            if (DefaultValue != null)
                SetToDefault();

            // иначе выставляем новый инстанс
            // HACK: должны иметь конструктор без параметров
            Value = Activator.CreateInstance(PropertyType);
        }

        protected virtual void SetToDefault()
        {
            // DefaultValue точно правильный - не нужно его еще раз прогонять через GetTrueType
            _value = DefaultValue;
        }

        protected virtual object GetValue()
        {
            return _value;
        }
        protected virtual void SetValue(object value)
        {
            _value = SerializationHelper.ConvertToTrueType(value, PropertyType);
        }

        protected virtual object GetDefaultValue()
        {
            return DefaultValue;
        }

        public void ChangePropertyType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            PropertyType = type;
        }

        #endregion
    }
}