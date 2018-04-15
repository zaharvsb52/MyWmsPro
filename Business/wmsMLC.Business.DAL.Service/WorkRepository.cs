using wmsMLC.Business.Objects;
using wmsMLC.General.DAL.Service;
using wmsMLC.General.DAL.Service.Telegrams;

namespace wmsMLC.Business.DAL.Service
{
    public abstract class WorkRepository : BaseHistoryRepository<Work, decimal>, IWorkRepository
    {
        public void FillByGroup(decimal workId, decimal groupId)
        {
            var workIdParam = new TransmitterParam { Name = "workId", Type = typeof(decimal), Value = workId };
            var groupIdParam = new TransmitterParam { Name = "groupId", Type = typeof(decimal), Value = groupId };
            var telegram = new RepoQueryTelegramWrapper(typeof(Work).Name, "FillByGroup", new[] { workIdParam, groupIdParam });
            ProcessTelegramm(telegram);
        }
    }

}