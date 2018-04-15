using System.Linq;
using wmsMLC.Business.DAL;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Business.Managers
{
    public class ReportFileManager : WMSBusinessObjectManager<ReportFile, decimal>, IReportFileManager
    {
        public byte[] GetReportFileBody(string fileName)
        {
            using (var repo = GetRepository<IReportFileRepository>())
                return repo.GetReportFileBody(fileName);
        }

        public ReportFile GetByReportFile(string reportFile, GetModeEnum mode = GetModeEnum.Partial)
        {
            if (string.IsNullOrEmpty(reportFile))
                return null;

            var reportFiles =
                GetFiltered(string.Format("{0} = '{1}'",
                    SourceNameHelper.Instance.GetPropertySourceName(typeof (ReportFile),
                        ReportFile.ReportFileFilePropertyName), reportFile), mode).ToArray();
            return reportFiles.Any() ? reportFiles[0] : null;
        }
    }
}