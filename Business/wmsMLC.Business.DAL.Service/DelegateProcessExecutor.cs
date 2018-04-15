using wmsMLC.Business.Objects.Processes;
using wmsMLC.General.DAL.Service;
using wmsMLC.General.DAL.Service.Telegrams;

namespace wmsMLC.Business.DAL.Service
{
    /// <summary>
    /// Делегирует выполенение процесса другому узлу (сервису)
    /// </summary>
    public class DelegateProcessExecutor : BaseRepository
    {
        public void Run(ExecutionContext context)
        {
            var pContext = new TransmitterParam { Name = "context", Type = typeof(ExecutionContext), IsOut = false };
            var telegram = new RepoQueryTelegramWrapper(typeof(BPProcess).Name, "Run", new[] { pContext });
            ProcessTelegramm(telegram);
        }
    }
}