using System.Collections.Generic;
using System.Linq;
using wmsMLC.Business.Objects;

namespace wmsMLC.Business.Managers
{
    public class InternalTrafficManager : WMSBusinessObjectManager<InternalTraffic, decimal>, IInternalTrafficManager
    {
        public bool FillMandant(InternalTraffic internalTraffic)
        {
            if (internalTraffic.ExternalTrafficID == null)
                return false;

            ExternalTraffic ext = null;
            using (var mgrExtTraffic = GetManager<ExternalTraffic>())
                ext = mgrExtTraffic.Get(internalTraffic.ExternalTrafficID);
            if (ext == null)
            {
                if (internalTraffic.MandantID == null)
                    return false;
                internalTraffic.MandantID = null;
                return false;
            }

            var list = ext.InternalTrafficList;
            if (list == null)
            {
                if (internalTraffic.MandantID == null)
                    return false;
                internalTraffic.MandantID = null;
                return false;
            }

            IEnumerable<InternalTraffic> result = list.OrderBy(x => x.DateIns).ThenBy(x => x.DateUpd);
            var mandant = result.Last().MandantID;

            if (mandant == internalTraffic.MandantID)
                return false;

            internalTraffic.MandantID = mandant;
            return true;
        }
    }
}