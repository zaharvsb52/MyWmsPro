using wmsMLC.Business.Objects;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Managers
{
    public interface IEventHeaderManager : ITrueBaseManager
    {
        //void RegEvent(ref EventHeader entity, EventParam paramLst);
        void RegEvent(ref EventHeader entity, EventDetail eventDetail);
    }
}