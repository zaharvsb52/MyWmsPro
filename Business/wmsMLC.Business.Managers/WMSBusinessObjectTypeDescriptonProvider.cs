//TODO: работа по instance-ам

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.ComponentModel;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Managers
{
    /// <summary>
    /// Провайдер информации о динамических объектах. Читает описания из DAL и конструирует TypeDescriptor-ы
    /// Реализует кэширование описаний.
    /// </summary>
    public class WMSBusinessObjectTypeDescriptonProvider : TypeDescriptionProvider
    {
        #region .  Fields  .
        private static readonly ConcurrentDictionary<Type, ICustomTypeDescriptor> _descriptorsCache
            = new ConcurrentDictionary<Type, ICustomTypeDescriptor>();
        #endregion

        #region .  Constructors  .
        public WMSBusinessObjectTypeDescriptonProvider() { }
        public WMSBusinessObjectTypeDescriptonProvider(TypeDescriptionProvider parent) : base(parent) { }
        #endregion

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            return _descriptorsCache.GetOrAdd(objectType, type => CreateTypeDescriptor(objectType, instance));
        }

        protected virtual ICustomTypeDescriptor CreateTypeDescriptor(Type objectType, object instance)
        {
            if (typeof (ICustomTypeDescriptor).IsAssignableFrom(objectType))
                return (ICustomTypeDescriptor)Activator.CreateInstance(objectType);

            // получаем описание извне
            using (var sysObjectManager = IoC.Instance.Resolve<ISysObjectManager>())
            {
                var typeDesc = sysObjectManager.GetTypeDescriptor(objectType);
                var customProperties = typeDesc.GetProperties().Cast<PropertyDescriptor>().ToArray();

                // пробегаем по зафиксированным свойствам и снимаем аттрибуты
                var staticProeprties = objectType.GetProperties();
                foreach (var staticProeprty in staticProeprties)
                {
                    // определяем имя св-ва в объекте
                    // если есть аттрибут и указано св-во отличное от св-ва в объекте
                    var hcAtt =
                        staticProeprty.GetCustomAttributes(typeof (HardCodedPropertyAttribute), true).FirstOrDefault()
                            as HardCodedPropertyAttribute;
                    var objectPropertyName = hcAtt == null || string.IsNullOrEmpty(hcAtt.PropertyName)
                        ? staticProeprty.Name
                        : hcAtt.PropertyName;

                    //NOTE: Eсли св-ва нет - ничего делать не нужно. Статические св-ва мы использовать не должны!
                    var existsProperty = customProperties.FirstOrDefault(i => objectPropertyName.EqIgnoreCase(i.Name));
                    if (existsProperty == null)
                        continue;

                    // если такое св-во уже есть - просто добавляем аттрибуты
                    var staticPropAtts = staticProeprty.GetCustomAttributes(true);
                    foreach (Attribute att in staticPropAtts)
                    {
                        // если аттрибут уже есть, то ругаемся!!! (возможно стоит реализовать marge)
                        if (existsProperty.Attributes[att.GetType()] != null)
                            throw new DeveloperException("Property {0} already contains attribute {1}",
                                staticProeprty.Name, att);

                        // все хорошо - переносим
                        var dynamicPropertyDesc = existsProperty as DynamicPropertyDescriptor;
                        if (dynamicPropertyDesc != null)
                            dynamicPropertyDesc.AddAttribute(att);
                    }
                }
                return typeDesc;
            }
        }

        public static void ClearCache()
        {
            foreach (var c in _descriptorsCache)
                TypeDescriptor.Refresh(c.Key);
            _descriptorsCache.Clear();
        }

        public static void AddTypeDescriptor(Type type, ICustomTypeDescriptor typeDescriptor)
        {
            _descriptorsCache.AddOrUpdate(type, t => typeDescriptor, (type1, descriptor) => typeDescriptor);
        }
    }
}