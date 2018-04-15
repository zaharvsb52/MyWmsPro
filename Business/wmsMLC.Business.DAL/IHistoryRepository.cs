using System.Collections.Generic;
using wmsMLC.Business.Objects;
using wmsMLC.General.DAL;

namespace wmsMLC.Business.DAL
{
    public interface IHistoryRepository<T> : IBaseRepository
    {
        IEnumerable<HistoryWrapper<T>> GetHistory(string filter, string attrentity);
    }
}