using System.Activities;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace wmsMLC.Activities.General
{
    public sealed class DynamicAssign<T> : CodeActivity
    {
        #region .  Fields & constants  .
        public const string ValuePropertyName = "Value";
        public const string PropertyNamePropertyName = "PropertyName";
        public const string ObjectPropertyName = "Object";
        private readonly PropertyDescriptorCollection _properties;
        #endregion

        #region .  Properties  .
        [RequiredArgument]
        [DefaultValue(null)]
        public InArgument<T> Object { get; set; }

        [RequiredArgument, DefaultValue(null)]
        public InArgument<string> PropertyName { get; set; }

        [RequiredArgument, DefaultValue(null)]
        public InArgument<object> Value { get; set; }
        #endregion

        public DynamicAssign()
        {
            _properties = TypeDescriptor.GetProperties(typeof(T));
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();

            var aValueType = typeof(object);
            if (Value != null)
                aValueType = this.Value.ArgumentType;
            var aValue = new RuntimeArgument(ValuePropertyName, aValueType, ArgumentDirection.In, true);
            metadata.Bind(Value, aValue);
            collection.Add(aValue);

            var aToType = typeof(object);
            if (Object != null)
                aToType = this.Object.ArgumentType;
            var aTo = new RuntimeArgument(ObjectPropertyName, aToType, ArgumentDirection.In, true);
            metadata.Bind(Object, aTo);
            collection.Add(aTo);

            var aProperty = new RuntimeArgument(PropertyNamePropertyName, typeof(string), ArgumentDirection.In, true);
            metadata.Bind(PropertyName, aProperty);
            collection.Add(aProperty);

            metadata.SetArgumentsCollection(collection);

            //TODO: проверки
            //            var propertyDesc = _properties[PropertyName.Get()];
            //            if (Value != null && PropertyName != null && !TypeHelper.AreTypesCompatible(this.Value.ArgumentType, this.To.ArgumentType))
            //            {
            //                metadata.AddValidationError(SR.TypeMismatchForAssign(this.Value.ArgumentType, this.To.ArgumentType, base.DisplayName));
            //            }
        }

        protected override void Execute(CodeActivityContext context)
        {
            // получаем все параметры
            var value = this.Value.Get(context);
            var obj = this.Object.Get(context);
            var propertyName = context.GetValue(this.PropertyName);

            // выставляем св-во
            _properties[propertyName].SetValue(obj, value);
        }
    }
}