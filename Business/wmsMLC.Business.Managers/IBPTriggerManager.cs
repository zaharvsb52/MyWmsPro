using System;
using System.Collections.Generic;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Managers
{
    public interface IBPTriggerManager : IBaseManager<BPTrigger>
    {
        IEnumerable<BPTrigger> GetByEntity(Type type, BPTriggerMode triggerrMode);
    }
}
