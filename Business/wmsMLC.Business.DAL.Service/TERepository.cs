using wmsMLC.Business.Objects;
using wmsMLC.General.DAL.Service;
using wmsMLC.General.DAL.Service.Telegrams;

namespace wmsMLC.Business.DAL.Service
{
    public abstract class TERepository : BaseHistoryRepository<TE, string>, ITERepository
    {
        public void ChangeStatus(string teCode, string operation)
        {
            var teCodeParam = new TransmitterParam { Name = "teCode", Type = typeof(string), Value = teCode, IsOut = false };
            var operationParam = new TransmitterParam { Name = "operation", Type = typeof(string), Value = operation, IsOut = false };
            var telegram = new RepoQueryTelegramWrapper(typeof(TE).Name, "ChangeStatus", new[] { teCodeParam, operationParam });
            ProcessTelegramm(telegram);
        }
    }
}
