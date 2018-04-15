using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BLToolkit.Aspects;
using BLToolkit.Reflection;
using wmsMLC.General.Resources;

namespace wmsMLC.General
{
    public abstract class SourceNameHelper
    {
        #region .  Singleton  .
        // ReSharper disable once InconsistentNaming
        // создаем новый инстанс через BLToolkit, чтобы иметь возможность кэшировать запросы
        private static readonly Lazy<SourceNameHelper> _instance = new Lazy<SourceNameHelper>(TypeAccessor.CreateInstance<SourceNameHelper>);
        protected SourceNameHelper() { }
        public static SourceNameHelper Instance
        {
            get { return _instance.Value; }
        }
        #endregion

        [Cache]
        public virtual string GetSourceName(Type type)
        {
            var trueType = type;

            //ТАК НЕЛЬЗЯ! МЕТОД НИЧЕГО НЕ ДОЛЖЕН ЗНАТЬ О ВАРИАНТАХ ЕГО ИСПОЛЬЗОВАНИЯ!
            // проверим, если это лист, то получим тип из него
            if (typeof(IList<>).IsAssignableFrom(type))
                trueType = type.GetGenericArguments()[0];

            var sourceName = trueType.Name;
            var customAtts = trueType.GetCustomAttributes(typeof(SourceNameAttribute), true);
            if (customAtts.Length == 1)
                return ((SourceNameAttribute)customAtts[0]).SourceName;
            if (customAtts.Length > 1)
                throw new DeveloperException(DeveloperExceptionResources.DuplicateAttribute, "SourceName");

            return sourceName;
        }

        [Cache]
        public virtual string GetPropertySourceName(Type type, string propertyName)
        {
            var props = TypeDescriptor.GetProperties(type).Cast<PropertyDescriptor>();
            var property = props.FirstOrDefault(i => propertyName.EqIgnoreCase(i.Name));
            var res = propertyName;
            if (property != null)
                res = GetPropertySourceName(property);
            return res;
        }

        //[Cache]
        //public virtual string GetPropertySourceName2(Type type, string propertyName)
        //{
        //    var props = TypeDescriptor.GetProperties(type).Cast<PropertyDescriptor>();
        //    var property = props.FirstOrDefault(i => propertyName.EqIgnoreCase(i.Name));
        //    var res = propertyName;
        //    if (property != null)
        //        res = GetPropertySourceName(property);
        //    return res;
        //}

        //public virtual Type GetPropertyType(Type type, string propertyName)
        //{
        //    var props = TypeDescriptor.GetProperties(type).Cast<PropertyDescriptor>();
        //    var property = props.FirstOrDefault(i => propertyName.EqIgnoreCase(i.Name));
        //    return property != null ? property.PropertyType : null;
        //}

        private static string GetPropertySourceName(PropertyDescriptor propertyDescriptor)
        {
            var sourceNameAtts = propertyDescriptor.Attributes[typeof(SourceNameAttribute)] as SourceNameAttribute;
            if (sourceNameAtts == null)
                return propertyDescriptor.Name;

            return sourceNameAtts.SourceName;
        }

        public virtual string GetPropertySourceNameWithNoVirtualFields(Type type, string propertyName)
        {
            var props = TypeDescriptor.GetProperties(type).Cast<PropertyDescriptor>();
            var property = props.FirstOrDefault(i => propertyName.EqIgnoreCase(i.Name));
            var res = propertyName;
            if (property != null)
                res = GetPropertySourceNameWithNoVirtualFields(property);
            return res;
        }

        private static string GetPropertySourceNameWithNoVirtualFields(PropertyDescriptor propertyDescriptor)
        {
            var sourceNameAtts = propertyDescriptor.Attributes[typeof(SourceNameAttribute)] as SourceNameAttribute;
            if (sourceNameAtts == null)
                return null;

            return sourceNameAtts.SourceName;
        }

        //[ClearCache]
        //public abstract void ClearCache();
    }
}