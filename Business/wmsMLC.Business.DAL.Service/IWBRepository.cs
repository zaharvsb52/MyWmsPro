using wmsMLC.Business.Objects;
using wmsMLC.General.DAL.Service;
using wmsMLC.General.DAL.Service.Telegrams;

namespace wmsMLC.Business.DAL.Service
{
    public abstract class IWBRepository : BaseHistoryRepository<IWB, decimal>, IIWBRepository
    {
        public void Activate(ref IWB entity)
        {
            var entityParam = new TransmitterParam { Name = "entity", Type = typeof(IWB), Value = entity, IsOut = true };
            var telegram = new RepoQueryTelegramWrapper(typeof(IWB).Name, "Activate", new[] { entityParam });
            ProcessTelegramm(telegram);
        }

        public void Cancel(ref IWB entity)
        {
            var entityParam = new TransmitterParam { Name = "entity", Type = typeof(IWB), Value = entity, IsOut = true };
            var telegram = new RepoQueryTelegramWrapper(typeof(IWB).Name, "Cancel", new[] { entityParam });
            ProcessTelegramm(telegram);
        }

        public void Complete(ref IWB entity)
        {
            var entityParam = new TransmitterParam { Name = "entity", Type = typeof(IWB), Value = entity, IsOut = true };
            var telegram = new RepoQueryTelegramWrapper(typeof(IWB).Name, "Complete", new[] { entityParam });
            ProcessTelegramm(telegram);
        }
    }
}
