using System.Collections.Generic;
using wmsMLC.Business.Objects;
using wmsMLC.General.DAL.Service;
using wmsMLC.General.DAL.Service.Telegrams;

namespace wmsMLC.Business.DAL.Service
{
    public abstract class BaseHistoryRepository<T, TKey> : BaseRepository<T, TKey>, IHistoryRepository<T>
        where T : class, new()
    {
        public const string GetHistoryActionName = "GetHistory";

        public IEnumerable<HistoryWrapper<T>> GetHistory(string filter, string attrentity)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(List<HistoryWrapper<T>>), IsOut = true };
            var filterParam = new TransmitterParam { Name = "filter", Type = typeof(string), Value = filter };
            var attrentityParam = new TransmitterParam { Name = "attrentity", Type = typeof(string), Value = attrentity };
            var telegram = new RepoQueryTelegramWrapper(typeof(T).Name, GetHistoryActionName, new[] { resultParam, filterParam, attrentityParam });
            ProcessTelegramm(telegram);
            return (List<HistoryWrapper<T>>)resultParam.Value;
        }
    }

    public abstract class BaseHistoryCacheableRepository<T, TKey> : CacheableRepository<T, TKey>, IHistoryRepository<T>
        where T : class, new()
    {
        public const string GetHistoryActionName = "GetHistory";

        public IEnumerable<HistoryWrapper<T>> GetHistory(string filter, string attrentity)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(List<HistoryWrapper<T>>), IsOut = true };
            var filterParam = new TransmitterParam { Name = "filter", Type = typeof(string), Value = filter };
            var attrentityParam = new TransmitterParam { Name = "attrentity", Type = typeof(string), Value = attrentity };
            var telegram = new RepoQueryTelegramWrapper(typeof(T).Name, GetHistoryActionName, new[] { resultParam, filterParam, attrentityParam });
            ProcessTelegramm(telegram);
            return (List<HistoryWrapper<T>>)resultParam.Value;
        }
    }
}