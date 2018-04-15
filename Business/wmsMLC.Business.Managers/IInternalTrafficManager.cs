using wmsMLC.Business.Objects;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Managers
{
    public interface IInternalTrafficManager : IBaseManager<InternalTraffic, decimal>
    {
        bool FillMandant(InternalTraffic internalTraffic);
    }
}