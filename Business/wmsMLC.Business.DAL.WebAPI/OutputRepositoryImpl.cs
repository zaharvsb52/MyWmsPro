using System.IO;
using wmsMLC.Business.Objects;
using wmsMLC.General.DAL.WebAPI;

namespace wmsMLC.Business.DAL.WebAPI
{
    public class OutputRepositoryImpl
    {
        public Stream GetReportPreview(Output task)
        {
            var helper = new WebAPIHelper();
            return helper.Post<Stream>("EPSPreviewReport", task);
        }

        public Output PrintReport(Output task)
        {
            var helper = new WebAPIHelper();
            return helper.Post<Output>("EPSPrintReport", task);
        }

        public OutputBatch PrintReportBatch(OutputBatch batch)
        {
            var helper = new WebAPIHelper();
            return helper.Post<OutputBatch>("EPSPrintReportBatch", batch);
        } 
    }
}