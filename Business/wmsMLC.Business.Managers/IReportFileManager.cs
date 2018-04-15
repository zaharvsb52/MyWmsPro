using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Business.Managers
{
    public interface IReportFileManager
    {
        byte[] GetReportFileBody(string fileName);
        ReportFile GetByReportFile(string reportFile, GetModeEnum mode = GetModeEnum.Partial);
    }
}
