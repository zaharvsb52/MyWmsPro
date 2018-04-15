using System;
using System.Text;
using FastReport;

namespace wmsMLC.EPS.wmsEPS.ExportFormat
{
    /// <summary>
    /// Интерфейс для классов - отчетов Fast Report
    /// </summary>
    public interface IExportFormat : IDisposable
    {
        void Export(Report report, System.IO.Stream stream);
        string FileExtension { get; }
        
        bool SupportsEncoding { get; }
        void SetEncoding(Encoding encoding);

        bool SupportsSpacelife { get; }
        void SetSpacelife(bool val);

    }
}
