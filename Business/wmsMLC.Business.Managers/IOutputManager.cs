using System.Collections.Generic;
using System.IO;
using wmsMLC.Business.Objects;

namespace wmsMLC.Business.Managers
{
    public interface IOutputManager : IHistoryManager<Output>
    {
        IEnumerable<Output> GetEpsOutputLst(int pRecCount, int pEpsHandler);

        Stream GetReportPreview(Output task);

        Output PrintReport(Output task);

        OutputBatch PrintReportBatch(OutputBatch batch);
    }
}
