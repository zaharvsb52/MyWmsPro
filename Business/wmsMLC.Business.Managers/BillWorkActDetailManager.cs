using System.Collections.Generic;
using System.Linq;
using wmsMLC.Business.Objects;

namespace wmsMLC.Business.Managers
{
    /// <summary>
    /// Менеджер детализации актов.
    /// </summary>
    public class BillWorkActDetailManager : WMSBusinessObjectManager<BillWorkActDetail, decimal>, IBillWorkActDetailManager
    {
        public List<BillOperationCause> GetCause(WMSBusinessObject entity)
        {
            if (entity == null)
                return null;

            var actD = entity as BillWorkActDetail;
            if (actD == null || actD.Operation2ContractID == null)
                return null;

            var filter = string.Format("OPERATION2CONTRACTID_R = {0}", actD.Operation2ContractID);
            List<BillOperationCause> res;

            using (var mgr = GetManager<BillOperationCause>())
                res = mgr.GetFiltered(filter).ToList();

            return res;
        }
    }
}