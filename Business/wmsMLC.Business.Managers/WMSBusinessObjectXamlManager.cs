using System;
using System.Collections.Generic;
using wmsMLC.Business.DAL;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Managers
{
    public abstract class WMSBusinessObjectXamlManager<T, TKey> : WMSBusinessObjectManager<T, TKey>, IXamlManager<T, TKey> where T : WMSBusinessObject
    {
        private static readonly HashSet<TKey> _cacheableObjectCodes = new HashSet<TKey>();
        private static readonly Dictionary<TKey, string> _objectCache = new Dictionary<TKey, string>();

        public virtual void SetXaml(TKey pKey, string xaml)
        {
            using (var repo = (IXamlRepository<T, TKey>)GetRepository())
                repo.SetXaml(pKey, xaml);
        }

        public virtual string GetXaml(TKey pKey)
        {
            return GetXaml(pKey, false);
        }

        public virtual string GetXaml(TKey pKey, bool allowGetFromCache)
        {
            if (pKey == null)
                throw new ArgumentNullException("pKey");

            if (!allowGetFromCache)
                return GetXamlWithoutCache(pKey);
            if (_objectCache.ContainsKey(pKey))
                return _objectCache[pKey];

            var xaml = GetXamlWithoutCache(pKey);

            if (IsObjectCachable(pKey))
                _objectCache.Add(pKey, xaml);

            return xaml;
        }

        private string GetXamlWithoutCache(TKey pKey)
        {
            using (var repo = (IXamlRepository<T, TKey>)GetRepository())
                return repo.GetXaml(pKey);
        }

        private static bool IsObjectCachable(TKey pKey)
        {
            return _cacheableObjectCodes.Contains(pKey);
        }

        public static void SetObjectCachable(TKey pKey)
        {
            if (_cacheableObjectCodes.Contains(pKey))
                throw new DeveloperException("Объект {0} уже помечен как кэшируемый", pKey);

            _cacheableObjectCodes.Add(pKey);
        }

        public static void RemoveObjectCachable(TKey pKey)
        {
            if (!_cacheableObjectCodes.Contains(pKey))
                throw new DeveloperException("Объект {0} не помечен как кэшируемый", pKey);

            _cacheableObjectCodes.Remove(pKey);
        }

        public static void ClearObjectCache(TKey pKey)
        {
            if (!_cacheableObjectCodes.Contains(pKey))
                throw new DeveloperException("Объект {0} не помечен как кэшируемый", pKey);

            if (_objectCache.ContainsKey(pKey))
                _objectCache.Remove(pKey);
        }

        void IXamlManager.SetXaml(object pKey, string xaml)
        {
            SetXaml((TKey)pKey, xaml);
        }

        public string GetXaml(object pKey, bool allowGetFromCache)
        {
            return GetXaml((TKey)pKey, allowGetFromCache);
        }

        string IXamlManager<T>.GetXaml(object pKey)
        {
            return GetXaml((TKey)pKey);
        }

        string IXamlManager.GetXaml(object pKey)
        {
            return GetXaml((TKey)pKey);
        }

        public void SetXaml(object pKey, string xaml)
        {
            SetXaml((TKey)pKey, xaml);
        }

        string IXamlManager.GetXaml(object pKey, bool allowGetFromCache)
        {
            return GetXaml((TKey)pKey, allowGetFromCache);
        }
    }

    public interface IXamlManager<T, in TKey> : IXamlManager<T>, IBaseManager<T>
    {
        void SetXaml(TKey pKey, string xaml);

        string GetXaml(TKey pKey, bool allowGetFromCache);
        string GetXaml(TKey pKey);
    }

    public interface IXamlManager<T> : IXamlManager, IBaseManager<T>
    {
        new void SetXaml(object pKey, string xaml);

        new string GetXaml(object pKey, bool allowGetFromCache);

        new string GetXaml(object pKey);
    }

    public interface IXamlManager : IBaseManager
    {
        void SetXaml(object pKey, string xaml);

        string GetXaml(object pKey, bool allowGetFromCache);

        string GetXaml(object pKey);
    }
}