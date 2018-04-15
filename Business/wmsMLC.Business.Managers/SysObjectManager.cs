using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BLToolkit.Aspects;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Managers
{
    internal class SysObjectManager : BusinessObjectManager<SysObject, decimal>, ISysObjectManager
    {
        #region .  Fields  .
        private static readonly ConcurrentDictionary<decimal, Lazy<Type>> _typesCache = new ConcurrentDictionary<decimal, Lazy<Type>>();
        private static readonly ConcurrentDictionary<string, Lazy<Type>> _customTypes = new ConcurrentDictionary<string, Lazy<Type>>();

        private static volatile bool _initilized;
        private static object _initializeLock = new object();
        #endregion

        #region .  Methods  .

        private void Initialize()
        {
            if (_initilized)
                return;

            lock (_initializeLock)
            {
                if (_initilized)
                    return;

                // до получения нормальных SysObject-ов подсовываем фиктивные
                WMSBusinessObjectTypeDescriptonProvider.AddTypeDescriptor(typeof(SysObject), new StubTypeDescriptor(typeof(SysObject)));
                WMSBusinessObjectTypeDescriptonProvider.AddTypeDescriptor(typeof(SysObjectExt), new StubTypeDescriptor(typeof(SysObjectExt)));

                base.GetAll();

                _initilized = true;

                // заменяем фиктивные на нормальные
                WMSBusinessObjectTypeDescriptonProvider.AddTypeDescriptor(typeof(SysObject), GetTypeDescriptor(typeof(SysObject)));
                WMSBusinessObjectTypeDescriptonProvider.AddTypeDescriptor(typeof(SysObjectExt), GetTypeDescriptor(typeof(SysObjectExt)));
            }
        }

        public override IEnumerable<SysObject> GetAll(GetModeEnum mode = GetModeEnum.Full)
        {
            Initialize();
            return base.GetAll(mode);
        }

        public override IEnumerable<SysObject> GetFiltered(string filter, GetModeEnum mode = GetModeEnum.Full)
        {
            Initialize();
            return base.GetFiltered(filter, mode);
        }

        public override IEnumerable<SysObject> GetFiltered(string filter, string attrentity)
        {
            Initialize();
            return base.GetFiltered(filter, attrentity);
        }

        public override SysObject Get(decimal key, GetModeEnum mode = GetModeEnum.Full)
        {
            return GetAll().FirstOrDefault(i => i.ObjectID == key);
        }

        protected override string GetAttrEntity(Type type, GetModeEnum mode)
        {
            //TODO: реализовать "короткий" формат
            return base.GetAttrEntity(type, mode);
        }

        public override SysObject Get(decimal key, string attrentity)
        {
            return GetAll().FirstOrDefault(i => i.ObjectID == key);
        }

        public Type GetTypeBySysObjectId(decimal id)
        {
            return _typesCache.GetOrAddSafe(id, i =>
                                            {
                                                var typeSysObj = Get(id);
                                                return GetTypeByName(typeSysObj.ObjectName);
                                            });
        }

        public void RegisterTypeName(string typeName, Type type)
        {
            _customTypes.AddOrUpdateSafe(typeName, s => type, (s, type1) => type);
        }

        public ICustomTypeDescriptor GetTypeDescriptor(Type type)
        {
            var sysObjNameAttr = Attribute.GetCustomAttribute(type, typeof(SysObjectNameAttribute)) as SysObjectNameAttribute;
            var typename = sysObjNameAttr != null
                ? sysObjNameAttr.SysObjectName
                : type.Name;

            var allItems = GetAll();
            var rootObject = allItems.FirstOrDefault(i => i.ObjectName.EqIgnoreCase(typename) && (!i.ObjectParentID.HasValue || i.ObjectParentID.Value == 0));
            if (rootObject == null)
                throw new DeveloperException("Сущность '{0}' не имеет описания в SysObject", typename);

            var items = allItems.Where(i => i.ObjectParentID == rootObject.ObjectID || i.ObjectID == rootObject.ObjectID);
            return EntityDescription.GetTypeDescriptor(this, type, items);
        }

        public Type GetTypeByName(string typeName)
        {
            //TODO: убрать отсюда HardCode. Как я пониял появился из-за ошибок в именовании в SysObject
            var tname = typeName == GlobalConfig.HackObjectName ? typeof(GlobalConfig).Name : typeName;
            // стандартный
            var type = Type.GetType(tname);
            if (type != null)
                return type;

            // явно зарегистрирован
            var customType = _customTypes.Keys.FirstOrDefault(i => i.EqIgnoreCase(tname));
            if (customType != null)
                return _customTypes[customType].Value;

            return null;
        }

        public override void ClearCache()
        {
            base.ClearCache();
            ClearCacheInternal();
        }

        public void LiteClearCache()
        {
            CacheAspect.ClearCache();
            ClearCacheInternal();
        }

        private void ClearCacheInternal()
        {
            // чистим внутренние объекты
            _typesCache.Clear();

            // чистим кэши в поставщике описаний
            _initilized = false;
            WMSBusinessObjectTypeDescriptonProvider.ClearCache();

            // заново инициализируем
            Initialize();
        }

        #endregion
   }

    public class StubTypeDescriptor : DynamicTypeDescriptor
    {
        public StubTypeDescriptor(Type ownerType)
            : base(ownerType)
        {
            var props = ownerType
                .GetProperties()
                .Where(i => i.GetCustomAttributes(typeof(HardCodedPropertyAttribute), true).Length > 0);
            // создаем коллекцию свойств вручную
            foreach (var p in props)
                AddProperty(new DynamicPropertyDescriptor(p.Name, null, ownerType, p.PropertyType, p.PropertyType.IsValueType ? Activator.CreateInstance(p.PropertyType) : null));
        }
    }
}