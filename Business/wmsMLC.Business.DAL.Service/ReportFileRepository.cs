using System;
using wmsMLC.Business.Objects;

namespace wmsMLC.Business.DAL.Service
{
    public abstract class ReportFileRepository : BaseHistoryRepository<ReportFile, decimal>, IReportFileRepository
    {
        public byte[] GetReportFileBody(string fileName)
        {
            throw new NotImplementedException();
        }
    }
}
