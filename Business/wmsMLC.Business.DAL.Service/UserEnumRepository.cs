using System.Collections.Generic;
using System.Xml;
using BLToolkit.Aspects;
using wmsMLC.Business.Objects;

namespace wmsMLC.Business.DAL.Service
{
    public abstract class UserEnumRepository : BaseHistoryCacheableRepository<UserEnum, decimal>
    {
        [Cache]
        public override List<UserEnum> GetFiltered(string filter, string attrentity)
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