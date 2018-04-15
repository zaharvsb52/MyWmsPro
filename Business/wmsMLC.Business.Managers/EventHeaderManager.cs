using System.Collections.Generic;
using wmsMLC.Business.DAL;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Business.Managers
{
    public class EventHeaderManager : WMSBusinessObjectManager<EventHeader, decimal>, IEventHeaderManager
    {
//        public void RegEvent(ref EventHeader entity, EventParam paramLst)
//        {
//            using (var repo = GetRepository<IEventHeaderRepository>())
//                repo.RegEvent(ref entity, paramLst);
//        }
        public void RegEvent(ref EventHeader entity, EventDetail eventDetail)
        {
            using (var repo = GetRepository<IEventHeaderRepository>())
                repo.RegEvent(ref entity, eventDetail);
        }
        public override void Insert(ref EventHeader entity)
        {
            throw new DeveloperException("You can't insert an EventHeader! Use 'RegEvent' method instead.");
        }

        public override void Insert(ref IEnumerable<EventHeader> entities)
        {
            throw new DeveloperException("You can't insert an EventHeader! Use 'RegEvent' method instead.");
        }

        public override void Delete(EventHeader entity)
        {
            
        }

        public override void Delete(IEnumerable<EventHeader> entities)
        {
            
        }
    }
}