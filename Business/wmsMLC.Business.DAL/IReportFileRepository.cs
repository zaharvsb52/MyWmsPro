using wmsMLC.Business.Objects;
using wmsMLC.General.DAL;

namespace wmsMLC.Business.DAL
{
    public interface IReportFileRepository : IRepository<ReportFile, decimal>
    {
        byte[] GetReportFileBody(string fileName);
    }
}
