using wmsMLC.Business.Objects;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class IWBRepository : BaseHistoryRepository<IWB, decimal>, IIWBRepository
    {
        public void Activate(ref IWB entity)
        {
            Update(entity);
        }

        public void Cancel(ref IWB entity)
        {
            Update(entity);
        }

        public void Complete(ref IWB entity)
        {
            Update(entity);
        }
    }
}
