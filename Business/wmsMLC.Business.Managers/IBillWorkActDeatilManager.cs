using System.Collections.Generic;
using wmsMLC.Business.Objects;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Managers
{
    public interface IBillWorkActDetailManager : IBaseManager<BillWorkActDetail, decimal>
    {
        List<BillOperationCause> GetCause(WMSBusinessObject entity);
    }
}