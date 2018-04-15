using System;
using System.Collections.Generic;
using System.IO;
using wmsMLC.Business.Objects;

namespace wmsMLC.Business.Managers
{
    public interface IReport2EntityManager : IDisposable
    {
        IEnumerable<Report2Entity> GetReportsForEntity(Type entityType);
        IEnumerable<Report> GetReports(Type entityType);
        PrintReportStatus PrintReport(decimal? mandantId, string reportCode, string printerCode = null, IEnumerable<OutputParam> paramExt = null);
        PrintReportStatus PrintReport(WMSBusinessObject entity, string reportCode, string printerCode = null, IEnumerable<OutputParam> paramExt = null);
        PrintReportStatus PrintReportBatch(WMSBusinessObject[] entities, string reportCode, string printerCode = null, IDictionary<WMSBusinessObject, IEnumerable<OutputParam>> paramExt = null);
        PrintStreamConfig GetDefaultPrinter(string tListParams);
        ReportRedefinition GetDefaultReport(string tListParams);
        Stream ExpReport(string reportCode, string fileName, string path, WMSBusinessObject entity, IEnumerable<OutputParam> paramExt = null);

        //HACK: Для тестов.
        PrintStreamConfig GetDefaultPrinter(string host, string login, string reportfilename, decimal? mandantcode);
        ReportRedefinition GetDefaultReport(WMSBusinessObject entity, string host, string reportfilename, decimal? mandantcode);
    }
}