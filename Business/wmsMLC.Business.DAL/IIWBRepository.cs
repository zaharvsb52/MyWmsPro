using wmsMLC.Business.Objects;
using wmsMLC.General.DAL;

namespace wmsMLC.Business.DAL
{
    public interface IIWBRepository : IRepository<IWB, decimal>
    {
        void Activate(ref IWB entity);
        void Cancel(ref IWB entity);
        void Complete(ref IWB entity);
    }
}
