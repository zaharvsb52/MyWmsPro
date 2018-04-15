#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="FRPng.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Olga Marysheva</Author>
/// <Date>10.10.2012 13:49:03</Date>
/// <Summary>Экпорт отчетов в формате файла изображений</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System;
using System.Text;
using FastReport;

namespace wmsMLC.EPS.wmsEPS.ExportFormat
{
    /// <summary>
    /// Экпорт отчета в формате изображения png
    /// </summary>
    public class FRPng : IExportFormat
    {
        private readonly FastReport.Export.Image.ImageExport _exp = new FastReport.Export.Image.ImageExport();
        public FRPng()
        {
            _exp.ImageFormat = FastReport.Export.Image.ImageExportFormat.Png;
            _exp.SeparateFiles = false;
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

        public string FileExtension { get { return "png"; } }

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