using System.Collections.Generic;
using System.Xml;
using BLToolkit.Aspects;

namespace wmsMLC.General.DAL.Oracle
{
    /// <summary>
    /// Репозиторий, методы чтения данных которого, кэшируются
    /// </summary>
    public abstract class CacheableRepository<T, TKey> : Repository<T, TKey>, ICacheableRepository
        where T : class, new()
    {
        protected CacheableRepository()
        {
            //IsNeedClearCacheBeforGetFiltered = true;
        }

        [Cache]
        public override T Get(TKey key, string attrentity)
        {
            return base.Get(key, attrentity);
        }

        [Cache]
        public override XmlDocument GetXml(TKey key, string attrentity)
        {
            return base.GetXml(key, attrentity);
        }

        [Cache]
        public override List<T> GetAll(string attrentity = null)
        {
            return base.GetAll(attrentity);
        }

        public override List<T> GetFiltered(string filter, string attrentity)
        {
            return base.GetFiltered(filter, attrentity);
        }

        public virtual void ClearCache()
        {
            CacheAspect.ClearCache(GetType());
        }
    }
}