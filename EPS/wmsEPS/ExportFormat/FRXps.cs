#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="FRXps.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Olga Marysheva</Author>
/// <Date>20.09.2012 14:16:09</Date>
/// <Summary>Экпорт отчетов в формате XPS</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System;
using System.Text;
using FastReport;

namespace wmsMLC.EPS.wmsEPS.ExportFormat
{
    /// <summary>
    /// Экпорт отчета в формате XPS - документ, хранения и просмотра спецификаций
    /// </summary>
    public class FRXps : IExportFormat
    {
        private readonly FastReport.Export.OoXML.XPSExport _exp = new FastReport.Export.OoXML.XPSExport();

        public void Export(Report report, System.IO.Stream stream)
        {
            _exp.Export(report, stream);
        }
        public void Dispose()
        {
            _exp.Clear();
            //Ошибка Dispose для экспорта в XPS
            //_exp.Dispose();
        }

        public string FileExtension { get { return "xps"; } }

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
