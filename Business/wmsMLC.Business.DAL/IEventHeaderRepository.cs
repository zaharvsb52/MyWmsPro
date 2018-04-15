using wmsMLC.Business.Objects;
using wmsMLC.General.DAL;

namespace wmsMLC.Business.DAL
{
    public interface IEventHeaderRepository : IRepository<EventHeader, decimal>
    {
        //void RegEvent(ref EventHeader entity, EventParam paramLst);
        void RegEvent(ref EventHeader entity, EventDetail eventDetail);
    }
}