using System;
using System.Text;
using FastReport;

namespace wmsMLC.EPS.wmsEPS.ExportFormat
{
    /// <summary>
    /// Сохранение отчета в формате предпросмотра
    /// </summary>
    public class FRFpx : IExportFormat
    {
        public void Export(Report report, System.IO.Stream stream)
        {
            report.SavePrepared(stream);
        }
        public void Dispose()
        {
        }
        
        public string FileExtension { get { return "fpx"; } }

        public bool SupportsEncoding { get { return false; } }
        public void SetEncoding(Encoding encoding)
        {
            throw new NotSupportedException();
        }
        public bool SupportsSpacelife { get { return false; } }
        public void SetSpacelife(bool val)
        {
            throw new NotSupportedException();
        }
    }
}