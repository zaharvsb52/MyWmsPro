#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="FRHtml.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Olga Marysheva</Author>
/// <Date>20.09.2012 13:45:11</Date>
/// <Summary>Экпорт отчетов в формате HTML</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System;
using System.Text;
using FastReport;

namespace wmsMLC.EPS.wmsEPS.ExportFormat
{
    /// <summary>
    /// Экпорт отчета в формате HTML
    /// </summary>
    public class FRHtml : IExportFormat
    {
        private readonly FastReport.Export.Html.HTMLExport _exp = new FastReport.Export.Html.HTMLExport();
        public FRHtml()
        {
            _exp.Format = FastReport.Export.Html.HTMLExportFormat.HTML;
        }

        public void Export(Report report, System.IO.Stream stream)
        {
            _exp.Export(report, stream);
        }

        public void Dispose()
        {
            _exp.Clear();
            _exp.Dispose();
        }

        public string FileExtension { get { return "html"; } }

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