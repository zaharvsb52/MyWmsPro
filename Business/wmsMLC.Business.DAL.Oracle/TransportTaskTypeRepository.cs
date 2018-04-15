using wmsMLC.Business.Objects;
using System;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class TransportTaskTypeRepository : BaseHistoryCacheableRepository<TransportTaskType, string>
    {
        protected override string GetPakageName(Type type)
        {
            return "pkgTransportTask";
        }
    }
}