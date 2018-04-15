#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="FROds.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Olga Marysheva</Author>
/// <Date>20.09.2012 13:52:49</Date>
/// <Summary>Экпорт отчетов в формате файла ODS (OpenOffice)</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System;
using System.Text;
using FastReport;

namespace wmsMLC.EPS.wmsEPS.ExportFormat
{
    /// <summary>
    /// Экпорт отчета в формате ODS -  электронные таблицы формата OpenDocument (OpenOffice)
    /// </summary>
    public class FROds : IExportFormat
    {
        private readonly FastReport.Export.Odf.ODSExport _exp = new FastReport.Export.Odf.ODSExport();

        public void Export(Report report, System.IO.Stream stream)
        {
            _exp.Export(report, stream);
        }
        public void Dispose()
        {
            _exp.Clear();
            _exp.Dispose();
        }

        public string FileExtension { get { return "ods"; } }

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