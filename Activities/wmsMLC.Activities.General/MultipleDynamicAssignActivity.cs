using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.General;

namespace wmsMLC.Activities.General
{
    public class MultipleDynamicAssignActivity<T> : NativeActivity
    {
        #region .  Fields & constants  .
        private readonly PropertyDescriptorCollection _propertycollection;
        private Dictionary<string, Argument> _properties; 
        #endregion .  Fields & constants  .

        public MultipleDynamicAssignActivity()
        {
            _propertycollection = TypeDescriptor.GetProperties(typeof(T));
            _properties = new Dictionary<string, Argument>();
        }

        #region .  Properties  .
        public Dictionary<string, Argument> Properties
        {
            get { return _properties; }
        }

        [RequiredArgument]
        [DefaultValue(null)]
        public InArgument<T> Object { get; set; }
        #endregion  .  Properties  .

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            ActivityHelpers.AcivityUseTypeValidator(metadata,typeof(T));

           var collection = new Collection<RuntimeArgument>();
            foreach (var key in Properties.Keys)
            {
                var argument = Properties[key];
                var runtimeArgument = new RuntimeArgument(key, argument.ArgumentType, argument.Direction);
                metadata.Bind(argument, runtimeArgument);
                collection.Add(runtimeArgument);
            }

            var aToType = typeof(object);
            if (Object != null)
                aToType = Object.ArgumentType;
            var aTo = new RuntimeArgument("Object", aToType, ArgumentDirection.In, true);
            metadata.Bind(Object, aTo);
            collection.Add(aTo);

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            // получаем все параметры
            var obj = Object.Get(context);

            foreach (var key in Properties.Keys)
            {
                var argument = Properties[key];
                if (argument == null)
                    throw new DeveloperException("MultipleDynamicAssignActivity<{0}>: Value is null for key '{1}' in Properties.", typeof(T).Name, key);

                var value = argument.Get(context);

                var propertyName = GetPropertyNameByKey(key);
                if (string.IsNullOrEmpty(propertyName))
                    throw new DeveloperException("MultipleDynamicAssignActivity<{0}>: Property name is null or empty for key '{1}'.", typeof(T).Name, key);

                var property = _propertycollection.Find(propertyName, true);
                if (property == null)
                    throw new DeveloperException("MultipleDynamicAssignActivity<{0}>: Property '{1}' not found (key='{2}').", typeof (T).Name, propertyName, key);

                property.SetValue(obj, value);
                //_propertycollection[GetPropertyNameByKey(key)].SetValue(obj, value);
            }
        }

        private string GetPropertyNameByKey(string key)
        {
            var ind = key.IndexOf('[');
            var trueKey = key.Substring(ind + 1, key.IndexOf(']') - ind - 1);
            return _propertycollection.Cast<PropertyDescriptor>()
                .Where(p => p.DisplayName.EqIgnoreCase(trueKey) || p.Name.EqIgnoreCase(trueKey)).Select(p => p.Name)
                    .Single();
        }
    }
}
