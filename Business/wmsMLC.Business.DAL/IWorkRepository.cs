using wmsMLC.Business.Objects;
using wmsMLC.General.DAL;

namespace wmsMLC.Business.DAL
{
    public interface IWorkRepository : IRepository<Work, decimal>
    {
        void FillByGroup(decimal workId, decimal groupId);
    }
}