using wmsMLC.Business.Objects;

namespace wmsMLC.Business.Managers
{
    public interface IIWBManager
    {
        void Activate(ref IWB entity);
        void Cancel(ref IWB entity);
        void Complete(ref IWB entity);
    }
}
