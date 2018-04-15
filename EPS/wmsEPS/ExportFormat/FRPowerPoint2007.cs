#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="FRPowerPoint2007.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Olga Marysheva</Author>
/// <Date>20.09.2012 14:11:47</Date>
/// <Summary>Экпорт отчетов в файлы Power Point 2007</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System;
using System.Text;
using FastReport;

namespace wmsMLC.EPS.wmsEPS.ExportFormat
{
    /// <summary>
    /// Экпорт отчета в формате Power Point 2007 (*.PPTX)
    /// </summary>
    public class FRPowerPoint2007 : IExportFormat
    {
        private readonly FastReport.Export.OoXML.PowerPoint2007Export _exp = new FastReport.Export.OoXML.PowerPoint2007Export();

        public void Export(Report report, System.IO.Stream stream)
        {
            _exp.Export(report, stream);
        }
        public void Dispose()
        {
            _exp.Clear();
            _exp.Dispose();
        }

        public string FileExtension { get { return "pptx"; } }

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
