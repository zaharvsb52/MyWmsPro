using wmsMLC.Business.Objects;
using wmsMLC.General.DAL;

namespace wmsMLC.Business.DAL
{
    public interface ITERepository : IRepository<TE, string>
    {
        void ChangeStatus(string teCode, string operation);
    }
}
