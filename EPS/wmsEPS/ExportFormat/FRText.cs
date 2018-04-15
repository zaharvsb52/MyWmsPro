#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="FRText.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Olga Marysheva</Author>
/// <Date>20.09.2012 14:31:53</Date>
/// <Summary>Экпорт отчетов в формате текстового файла</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System;
using System.IO;
using System.Text;
using FastReport;

namespace wmsMLC.EPS.wmsEPS.ExportFormat
{
    /// <summary>
    /// Экпорт отчета в формате текста
    /// </summary>
    public class FRText : IExportFormat
    {
        private readonly FastReport.Export.Text.TextExport _exp = new FastReport.Export.Text.TextExport();
        private Encoding _encoding = Encoding.UTF8;
        private bool _spacelife = false;

        public FRText()
        {
            _exp.PageBreaks = false;
            _exp.EmptyLines = false;
            _exp.DataOnly = false;

            _exp.Frames = false;
            _exp.Encoding = new UnicodeEncoding();
        }

        public void Export(Report report, System.IO.Stream stream)
        {
            if (_spacelife)
            {
                using (var ms = new MemoryStream())
                {
                    _exp.Encoding = Encoding.UTF8;
                    _exp.Export(report, ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    using (var sr = new StreamReader(ms, Encoding.UTF8))
                    using (var sw = new StreamWriter(stream, _encoding))
                    {
                        var content = sr.ReadToEnd().Replace("$endline$", string.Empty);
                        sw.Write(content);                        
                    }
                }
            }
            else
            {
                _exp.Encoding = _encoding;
                _exp.Export(report, stream);
            }
        }

        public void Dispose()
        {
            _exp.Clear();
            _exp.Dispose();
        }

        public string FileExtension { get { return "txt"; } }
        public bool SupportsEncoding { get { return true; } }
        public void SetEncoding(Encoding encoding)
        {
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            _encoding = encoding;
        }

        public bool SupportsSpacelife { get { return true; } }
        public void SetSpacelife(bool val)
        {
            _spacelife = val;
        }
    }
}
