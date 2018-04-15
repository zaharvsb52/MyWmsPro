using System.Collections.Generic;
using System.IO;
using wmsMLC.Business.DAL;
using wmsMLC.Business.Objects;

namespace wmsMLC.Business.Managers
{
    public class OutputManager : WMSBusinessObjectManager<Output, decimal>, IOutputManager
    {
        public IEnumerable<Output> GetEpsOutputLst(int pRecCount, int pEpsHandler)
        {
            using (var repo = GetRepository<IOutputRepository>())
                return repo.GetEpsOutputLst(pRecCount, pEpsHandler);
        }

        public Stream GetReportPreview(Output task)
        {
            using (var repo = GetRepository<IOutputRepository>())
                return repo.GetReportPreview(task);
        }

        public Output PrintReport(Output task)
        {
            using (var repo = GetRepository<IOutputRepository>())
                return repo.PrintReport(task);
        }

        public OutputBatch PrintReportBatch(OutputBatch batch)
        {
            using (var repo = GetRepository<IOutputRepository>())
                return repo.PrintReportBatch(batch);
        }
    }
}