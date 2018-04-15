#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="FRBmp.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Olga Marysheva</Author>
/// <Date>10.10.2012 13:42:29</Date>
/// <Summary>Экпорт отчетов в формате файла изображений</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System;
using System.Text;
using FastReport;

namespace wmsMLC.EPS.wmsEPS.ExportFormat
{
    /// <summary>
    /// Экпорт отчета в формате изображения - bmp
    /// </summary>
    public class FRBmp : IExportFormat
    {
        private readonly FastReport.Export.Image.ImageExport _exp = new FastReport.Export.Image.ImageExport();
        public FRBmp()
        {
            _exp.ImageFormat = FastReport.Export.Image.ImageExportFormat.Bmp;
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

        public string FileExtension { get { return "bmp"; } }
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