using wmsMLC.Business.Objects;
using wmsMLC.General.DAL;

namespace wmsMLC.Business.DAL
{
    public interface ITransportTaskRepository : IRepository<TransportTask, decimal>
    {
        TransportTask GetByClientCode(string clientCode, out int count);
        TransportTask GetNextByClientCode(string clientCode, decimal previousTtaskCode);
        void Activate(ref TransportTask entity);
        void Cancel(ref TransportTask entity);
        void Complete(ref TransportTask entity);
    }
}
