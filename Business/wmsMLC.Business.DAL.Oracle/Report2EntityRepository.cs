using System.Xml;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class Report2EntityRepository : BaseHistoryRepository<Report2Entity, decimal>, IReport2EntityRepository
    {
        public PrintStreamConfig GetDefaultPrinter(string tListParams)
        {
            return RunManualDbOperation(db =>
                {
                    var stm = string.Format("select {0}({1}) from dual", "PKGPRINTSTREAMCONFIG.getDefaultPrinter", tListParams);
                    var resXml = db.SetCommand(stm, null).ExecuteScalar<XmlDocument>();
                    return resXml == null ? null : XmlDocumentConverter.ConvertTo<PrintStreamConfig>(resXml);
                });
        }

        public ReportRedefinition GetDefaultReport(string tListParams)
        {
            return RunManualDbOperation(db =>
            {
                var stm = string.Format("select {0}({1}) from dual", "pkgReportRedefinition.getDefaultReport", tListParams);
                var resXml = db.SetCommand(stm, null).ExecuteScalar<XmlDocument>();
                return resXml == null ? null : XmlDocumentConverter.ConvertTo<ReportRedefinition>(resXml);
            });
        }
    }
}
