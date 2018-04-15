using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.Resources;

namespace wmsMLC.Business.Objects
{
    /// <summary>
    /// Базовый бизнес-объект WMS системы.
    /// Реализует логику получения описания объектов.
    /// </summary>
    public abstract class WMSBusinessObject : EditableBusinessObject, IKeyHandler, ICloneable
    {
        #region .  Constants & Fields  .
        public const string UserInsPropertyName = "UserIns";
        public const string DateInsPropertyName = "DateIns";
        public const string UserUpdPropertyName = "UserUpd";
        public const string DateUpdPropertyName = "DateUpd";
        public const string TransactPropertyName = "Transact";

        private static readonly ConcurrentDictionary<Type, Lazy<string>> _primaryKeyPropertyCache = new ConcurrentDictionary<Type, Lazy<string>>();
        #endregion

        protected WMSBusinessObject()
        {
            //TODO: Пока эксперементируем с историей - разрешаем добавлять новые св-ва
            UnknownPropertySet = UnknownPropertySetMode.AddNewProperty;
        }

        #region .  Properties  .
        /// <summary>
        /// Идентификатор пользователя добавившего запись
        /// </summary>
        [HardCodedProperty]
        public string UserIns
        {
            get { return GetProperty<string>(UserInsPropertyName); }
        }

        /// <summary>
        /// Дата добавления записи
        /// </summary>
        [HardCodedProperty]
        public DateTime? DateIns
        {
            get { return GetProperty<DateTime?>(DateInsPropertyName); }
        }

        /// <summary>
        /// Идентификатор пользователя обновившего запись
        /// </summary>
        [HardCodedProperty]
        public string UserUpd
        {
            get { return GetProperty<string>(UserUpdPropertyName); }
        }

        /// <summary>
        /// Дата изменения записи
        /// </summary>
        [HardCodedProperty]
        public DateTime? DateUpd
        {
            get { return GetProperty<DateTime?>(DateUpdPropertyName); }
        }

        /// <summary>
        /// Количество изменений записи
        /// </summary>
        [HardCodedProperty]
        public decimal? Transact
        {
            get { return GetProperty<decimal?>(TransactPropertyName); }
        }

        #endregion

        #region .  Methods  .

        public virtual bool IsPersisted()
        {
            return Transact.HasValue;
        }

        protected override CustomPropertyCollection CreateCustomProperties()
        {
            var properties = TypeDescriptor.GetProperties(GetEntityType());
            var customProperties = new CustomPropertyCollection();
            foreach (DynamicPropertyDescriptor property in properties)
            {
                CustomProperty customProperty = null;
                var att = property.GetAttributeByTypeFast<VirtualAttribute>();
                customProperty = att != null
                    ? new VirtualCalcProperty(property.Name, property.PropertyType, property.DefaultValue, att.VirtualValue, this)
                    : new CustomProperty(property.Name, property.PropertyType, property.DefaultValue);

                customProperties[property.Name] = customProperty;
            }
            return customProperties;
        }

        /// <summary>
        /// Определение сущности, для которой будут полечены св-ва. По-умолчанию сам класс.
        /// Данный метод может быть использован во всяческих Wrapper-ах, которым необходимо дополнить св-ва
        /// </summary>
        protected virtual Type GetEntityType()
        {
            return GetType();
        }

        public virtual bool HasPrimaryKey()
        {
            return true;
        }
        #endregion

        #region .  IKeyHandler  .

        public object GetKey()
        {
            return GetKey_Internal();
        }

        public TKey GetKey<TKey>()
        {
            return (TKey) GetKey();
        }

        public void SetKey(object o)
        {
            var pk = GetPrimaryKeyPropertyName();
            SetProperty(pk, o);
        }

        public string GetPrimaryKeyPropertyName()
        {
            return GetPrimaryKeyPropertyName(GetType());
        }

        protected virtual object GetKey_Internal()
        {
            var pk = GetPrimaryKeyPropertyName();
            return GetProperty(pk);
        }

        public static string GetPrimaryKeyPropertyName(Type type)
        {
            return _primaryKeyPropertyCache.GetOrAddSafe(type, t =>
            {
                //ищем св-ва, отмеченные, как первичный ключ
                var properties = TypeDescriptor.GetProperties(type)
                    .Cast<PropertyDescriptor>()
                    .Where(i => i.Attributes.Cast<Attribute>().Any(j => j is PrimaryKeyAttribute))
                    .ToArray();

                // какой либо ключ обязательно должен быть
                if (properties.Length == 0)
                    throw new DeveloperException(DeveloperExceptionResources.CantFindPrimaryKey);

                // много ключей
                if (properties.Length > 1)
                    throw new DeveloperException(DeveloperExceptionResources.СompositePrimaryKeysIsNotImplementedYet);

                // один ключ
                // NOTE: ключ может быть пустым!
                return properties[0].Name;
            });
        }

        #endregion

        public override string ToString()
        {
            var displayName = GetType().GetDisplayName();
            var key = GetKey_Internal();
            return displayName + (key == null ? string.Empty : string.Format(" '{0}'", key));
        }

        public virtual object Clone()
        {
            var clon = (WMSBusinessObject)Activator.CreateInstance(GetType());
            var properties = TypeDescriptor.GetProperties(this).Cast<PropertyDescriptor>();
            foreach (var p in properties)
            {
                var value = p.GetValue(this);
                if (value == null || value.GetType().IsPrimitive || value.GetType().IsValueType)
                {
                    clon.SetProperty(p.Name, value);
                }
                else
                {
                    var clonableClon = value as ICloneable;
                    if (clonableClon != null)
                        clon.SetProperty(p.Name, clonableClon.Clone());
                    else
                        throw new NotImplementedException();
                }
            }
            return clon;
        }

        /// <summary>
        /// Копирование объектов WMSBusinessObject.
        /// </summary>
        public static void Copy(WMSBusinessObject objectFrom, WMSBusinessObject objectTo)
        {
            if (objectFrom == null || objectTo == null)
                return;

            var properties = TypeDescriptor.GetProperties(objectTo);
            foreach (var basepd in TypeDescriptor.GetProperties(objectFrom).Cast<PropertyDescriptor>().Where(p => !p.IsReadOnly))
            {
                var property = properties.Find(basepd.Name, false);
                if (property != null && !property.IsReadOnly)
                    property.SetValue(objectTo, basepd.GetValue(objectFrom));
            }
        }
    }
}