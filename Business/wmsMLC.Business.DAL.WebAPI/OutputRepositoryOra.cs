using System.IO;
using wmsMLC.Business.Objects;

namespace wmsMLC.Business.DAL.WebAPI
{
    public abstract class OutputRepositoryOra : Oracle.OutputRepository
    {
        private readonly OutputRepositoryImpl _impl = new OutputRepositoryImpl();

        public override Stream GetReportPreview(Output task)
        {
            return _impl.GetReportPreview(task);
        }

        public override Output PrintReport(Output task)
        {
            return _impl.PrintReport(task);
        }

        public override OutputBatch PrintReportBatch(OutputBatch batch)
        {
            return _impl.PrintReportBatch(batch);
        }
    }
}