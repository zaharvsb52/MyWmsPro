using System.Collections.Generic;
using System.Xml;
using BLToolkit.Aspects;
using wmsMLC.Business.Objects;
using wmsMLC.General.DAL;

namespace wmsMLC.Business.DAL.Service
{
    /// <summary>
    /// Репозиторий отчетов
    /// </summary>
    public abstract class ReportRepository : BaseHistoryRepository<Report, string>, ICacheableRepository
    {
        [Cache]
        public override List<Report> GetFiltered(string filter, string attrentity)
        {
            return base.GetFiltered(filter, attrentity);
        }

        [Cache]
        public override List<XmlDocument> GetXmlFiltered(string filter, string attrentity)
        {
            return base.GetXmlFiltered(filter, attrentity);
        }

        public void ClearCache()
        {
            CacheAspect.ClearCache(GetType());
        }
        
    }
}