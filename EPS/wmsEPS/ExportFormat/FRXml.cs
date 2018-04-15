#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="FRXml.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Olga Marysheva</Author>
/// <Date>20.09.2012 14:23:13</Date>
/// <Summary>Экпорт отчетов в формате XML (excel 2003+)</Summary>                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    </Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System;
using System.IO;
using System.Text;
using System.Xml;
using FastReport;

namespace wmsMLC.EPS.wmsEPS.ExportFormat
{
    /// <summary>
    /// Экпорт отчета в формате XML (excel 2003+)
    /// </summary>
    public class FRXml : IExportFormat
    {
        private readonly FastReport.Export.Xml.XMLExport _exp = new FastReport.Export.Xml.XMLExport();
        private Encoding _encoding = Encoding.UTF8;

        public void Export(Report report, System.IO.Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                _exp.Export(report, ms);
                ms.Seek(0, SeekOrigin.Begin);
                var doc = new XmlDocument();
                doc.Load(ms);
                
                using (var writer = new StreamWriter(stream, _encoding))
                {
                    doc.Save(writer);
                }
            }
        }

        public void Dispose()
        {
            _exp.Clear();
            _exp.Dispose();
        }

        public string FileExtension { get { return "xls"; } }
        public bool SupportsEncoding { get { return true; } }
        public void SetEncoding(Encoding encoding)
        {
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            _encoding = encoding;
        }

        public bool SupportsSpacelife { get { return false; } }
        public void SetSpacelife(bool val)
        {
            throw new NotSupportedException();
        }
    }
}
