using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BLToolkit.Reflection;
using wmsMLC.Business.Managers.Validation.Attributes;
using wmsMLC.General.BL;


namespace wmsMLC.Business.Managers.Validation
{
    public abstract class DefaultValueSetter
    {
        #region .  Singleton  .

        public const string DefaultStrategyName = "default.value";
        // создаем новый инстанс через BLToolkit
        private static readonly Lazy<DefaultValueSetter> _instance = new Lazy<DefaultValueSetter>(TypeAccessor.CreateInstance<DefaultValueSetter>);
        protected DefaultValueSetter() { }
        public static DefaultValueSetter Instance
        {
            get { return _instance.Value; }
        }
        #endregion

        #region .  Methods  .

        public void SetDefaultValues(BusinessObject obj)
        {
            var properties = TypeDescriptor.GetProperties(obj);
            foreach (PropertyDescriptor p in properties)
            {
                if (typeof (IList).IsAssignableFrom(p.PropertyType) && !typeof (string).IsAssignableFrom(p.PropertyType))
                {
                    var list = p.GetValue(obj) as IEnumerable<BusinessObject>;
                    if (list != null)
                    {
                        foreach (var l in list)
                            SetDefaultValues(l);
                    }
                }
                else
                {
                    // проверим атрибут стратегии default.value
                    var validateAttributes = p.Attributes.OfType<WMSValidateAttribute>().ToList();
                    if (validateAttributes.Count > 0)
                    {
                        var dv = validateAttributes.FirstOrDefault(i => i.GetName().Equals(DefaultStrategyName));
                        if (dv == null) continue;
                        var value = p.GetValue(obj);
                        if ((value == null) || (value is DateTime && value.Equals(DateTime.MinValue)) ||
                            (value is Guid && value.Equals(Guid.Empty)) ||
                            (value is string && string.IsNullOrEmpty((string) value)))
                            p.SetValue(obj, obj.GetPropertyDefaultValue(p.Name));
                    }
                }
            }
        }

        #endregion
    }
}
