#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="FRRtf.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Olga Marysheva</Author>
/// <Date>20.09.2012 14:14:06</Date>
/// <Summary>Экпорт отчетов в формате RTF</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System;
using System.Text;
using FastReport;

namespace wmsMLC.EPS.wmsEPS.ExportFormat
{
    /// <summary>
    /// Экпорт отчета в формате текстовых документов (RTF)
    /// </summary>
    public class FRRtf : IExportFormat
    {
        private readonly FastReport.Export.RichText.RTFExport _exp = new FastReport.Export.RichText.RTFExport();

        public void Export(Report report, System.IO.Stream stream)
        {
            _exp.Export(report, stream);
        }
        public void Dispose()
        {
            _exp.Clear();
            _exp.Dispose();
        }

        public string FileExtension { get { return "rtf"; } }

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
