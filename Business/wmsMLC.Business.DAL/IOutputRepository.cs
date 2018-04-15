using System.Collections.Generic;
using System.IO;
using wmsMLC.Business.Objects;
using wmsMLC.General.DAL;

namespace wmsMLC.Business.DAL
{
    public interface IOutputRepository : IRepository<Output, decimal>
    {
        IEnumerable<Output> GetEpsOutputLst(int pRecCount, int pEpsHandler);

        Stream GetReportPreview(Output task);

        Output PrintReport(Output task);
        OutputBatch PrintReportBatch(OutputBatch batch);
    }
}
