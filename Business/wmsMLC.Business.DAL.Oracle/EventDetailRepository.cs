using wmsMLC.Business.Objects;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class EventDetailRepository : BaseHistoryRepository<EventDetail, decimal>
    {
        protected override string GetPakageName(System.Type type)
        {
            return "pkgEventHeader";
        }
    }
}