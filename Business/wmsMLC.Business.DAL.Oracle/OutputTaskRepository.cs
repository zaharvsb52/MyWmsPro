using System;
using wmsMLC.Business.Objects;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class OutputTaskRepository : BaseHistoryRepository<OutputTask, decimal>
    {
        protected override string GetPakageName(Type type)
        {
            return "PKGOUTPUT";
        }
    }
}
