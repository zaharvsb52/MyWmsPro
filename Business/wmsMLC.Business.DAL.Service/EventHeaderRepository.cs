using wmsMLC.Business.Objects;
using wmsMLC.General.DAL.Service;
using wmsMLC.General.DAL.Service.Telegrams;

namespace wmsMLC.Business.DAL.Service
{
    public abstract class EventHeaderRepository : BaseHistoryRepository<EventHeader, decimal>, IEventHeaderRepository
    {
//        public void RegEvent(ref EventHeader entity, EventParam paramLst)
//        {
//            var paramLstParam = new TransmitterParam { Name = "paramLst", Type = typeof(EventParam), Value = paramLst };
//            var entityParam = new TransmitterParam { Name = "entity", Type = typeof(EventHeader), Value = entity, IsOut = true };
//            var telegram = new RepoQueryTelegramWrapper(typeof(EventHeader).Name, "RegEvent", new[] { entityParam, paramLstParam });
//            ProcessTelegramm(telegram);
//            entity = (EventHeader)entityParam.Value;
//        }

        public void RegEvent(ref EventHeader entity, EventDetail eventDetail)
        {
            var paramLstParam = new TransmitterParam { Name = "eventDetail", Type = typeof(EventDetail), Value = eventDetail };
            var entityParam = new TransmitterParam { Name = "entity", Type = typeof(EventHeader), Value = entity, IsOut = true };
            var telegram = new RepoQueryTelegramWrapper(typeof(EventHeader).Name, "RegEvent", new[] { entityParam, paramLstParam });
            ProcessTelegramm(telegram);
            entity = (EventHeader)entityParam.Value;
        }
    }
}