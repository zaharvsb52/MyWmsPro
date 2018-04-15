#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="FRMhtml.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Olga Marysheva</Author>
/// <Date>10.10.2012 13:38:55</Date>
/// <Summary>Экпорт отчетов в формате MessageHTML</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System;
using System.Text;
using FastReport;

namespace wmsMLC.EPS.wmsEPS.ExportFormat
{
    /// <summary>
    /// Экпорт отчета в формате MessageHTML
    /// </summary>
    public class FRMhtml : IExportFormat
    {
        private readonly FastReport.Export.Html.HTMLExport _exp = new FastReport.Export.Html.HTMLExport();
        public FRMhtml()
        {
            _exp.Format = FastReport.Export.Html.HTMLExportFormat.MessageHTML;
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