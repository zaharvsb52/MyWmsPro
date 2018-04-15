using System.Collections.Generic;
using System.Xml;
using BLToolkit.Aspects;
using wmsMLC.Business.Objects;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class SysObjectRepository : BaseHistoryCacheableRepository<SysObject, decimal>
    {
        protected SysObjectRepository()
        {
            //IsNeedClearCacheBeforGetFiltered = false;
        }

        public const string ClearCacheActionName = "ClearCache";

        public override void ClearCache()
        {
            // чистим весь имеющийся кэш
            CacheAspect.ClearCache();
        }

        public override List<XmlDocument> GetXmlFiltered(string filter, string attrentity)
        {
            // если получаем все объекты - лезем в кэш
            if (string.IsNullOrEmpty(filter) && string.IsNullOrEmpty(attrentity))
                return GetXmlFilteredWithCache(filter, attrentity);

            // иначе в DB
            return base.GetXmlFiltered(filter, attrentity);
        }

        [Cache]
        protected virtual List<XmlDocument> GetXmlFilteredWithCache(string filter, string attrentity)
        {
            return base.GetXmlFiltered(filter, attrentity);
        }
    }
}