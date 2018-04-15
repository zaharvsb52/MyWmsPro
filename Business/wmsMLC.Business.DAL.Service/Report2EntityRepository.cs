using wmsMLC.Business.Objects;
using wmsMLC.General.DAL.Service;
using wmsMLC.General.DAL.Service.Telegrams;

namespace wmsMLC.Business.DAL.Service
{
    public abstract class Report2EntityRepository : BaseHistoryRepository<Report2Entity, decimal>, IReport2EntityRepository
    {
        public PrintStreamConfig GetDefaultPrinter(string tListParams)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(PrintStreamConfig), IsOut = true };
            var tListParamsParam = new TransmitterParam { Name = "tListParams", Type = typeof(string), IsOut = false, Value = tListParams };
            var telegram = new RepoQueryTelegramWrapper(typeof(Report2Entity).Name, "GetDefaultPrinter", new[] { resultParam, tListParamsParam });
            ProcessTelegramm(telegram);
            return (PrintStreamConfig)resultParam.Value;
        }

        public ReportRedefinition GetDefaultReport(string tListParams)
        {
            var resultParam = new TransmitterParam { Name = "result", Type = typeof(ReportRedefinition), IsOut = true };
            var tListParamsParam = new TransmitterParam { Name = "tListParams", Type = typeof(string), IsOut = false, Value = tListParams };
            var telegram = new RepoQueryTelegramWrapper(typeof(Report2Entity).Name, "GetDefaultReport", new[] { resultParam, tListParamsParam });
            ProcessTelegramm(telegram);
            return (ReportRedefinition)resultParam.Value;
        }
    }
}
