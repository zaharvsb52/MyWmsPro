using System.Collections.Generic;
using System.Xml;
using BLToolkit.Aspects;

namespace wmsMLC.Business.DAL.Service
{
    /// <summary>
    /// Репозиторий для Lookup-ны сущностией, которые можно кэшировать
    /// </summary>
    public abstract class HistoryAndFullCacheableRepository<T, TKey> : BaseHistoryCacheableRepository<T, TKey>
        where T : class, new()
    {
        [Cache]
        public override List<T> GetFiltered(string filter, string attrentity)
        {
            return base.GetFiltered(filter, attrentity);
        }

        [Cache]
        public override List<XmlDocument> GetXmlFiltered(string filter, string attrentity)
        {
            return base.GetXmlFiltered(filter, attrentity);
        }

    }
}