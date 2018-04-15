#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="wmsEPSFastReport.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Olga Marysheva</Author>
/// <Date>18.09.2012 17:16:46</Date>
/// <Summary>Работа с отчетами Fast Report</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System;
using System.Linq;
using FastReport;
using System.IO;
using wmsMLC.EPS.wmsEPS.ExportFormat;
using System.Collections.Generic;

namespace wmsMLC.EPS.wmsEPS
{
    /// <summary>
    /// Функционал для работы с FastReport
    /// </summary>
    public class EpsFastReport : IDisposable
    {
        /// <summary>
        /// Отчет FastReport.
        /// </summary>
        private readonly Report _report;

        /// <summary>
        /// Логическрое имя отчета.
        /// </summary>
        private readonly string _reportName;
        public string ReportName
        {
            get { return _reportName; }
        }

        private readonly string _reportFileName;
        public string ReportFileName
        {
            get { return _reportFileName; }
        }

        /// <summary>
        /// Список stream-ов и типов при разных экпортах.
        /// </summary>
        private readonly Dictionary<string, EpsStreamType> _exportStreams;

        /// <summary>
        /// Список типов для экпортах.
        /// </summary>
        private readonly List<ExportType> _exportTypes;

        private readonly EnvironmentSettings _environmentSettings;

        /// <summary>
        /// Работа с отчетами FastReport.
        /// </summary>
        public EpsFastReport(string reportName, string reportFileName, string connectionString, string tempFolder)
        {
            if (string.IsNullOrEmpty(reportName))
                throw new ArgumentNullException("reportName");

            if (string.IsNullOrEmpty(reportFileName))
                throw new ArgumentNullException("reportFileName");

            if (!(File.Exists(reportFileName)))
                throw new FileNotFoundException(reportFileName);

            if (string.IsNullOrEmpty(tempFolder))
                throw new ArgumentNullException("tempFolder");

            _environmentSettings = new EnvironmentSettings { ReportSettings = { ShowProgress = false } };
            // убираем окно прогресса
            FastReport.Utils.Config.ReportSettings.ShowProgress = false;

            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }
            FastReport.Utils.Config.TempFolder = tempFolder;

            _reportName = reportName;
            _reportFileName = reportFileName;

            _report = new Report();
            _report.Load(reportFileName);

            if (!string.IsNullOrEmpty(connectionString))
            {
                for (var i = 0; i < _report.Dictionary.Connections.Count; i++)
                {
                    _report.Dictionary.Connections[i].ConnectionString = connectionString;
                }
            }

            _exportStreams = new Dictionary<string, EpsStreamType>();
            _exportTypes = new List<ExportType>();
        }

        /// <summary>
        /// Добавление параметра в отчет report.
        /// </summary>
        /// <param name="nameParameter">имя параметра</param>
        /// <param name="value">значение параметра</param>
        public void AddParameters(string nameParameter, object value)
        {
            _report.SetParameterValue(nameParameter, value);
        }

        /// <summary>
        /// Формирование отчета.
        /// </summary>
        public void Prepare()
        {
            _report.Prepare();
        }

        /// <summary>
        /// Добавление типов для экпорта.
        /// </summary>
        public void AddExportType(ExportType et)
        {
            if (et == null)
                throw new ArgumentNullException("et");

            // TODO: Make set
            if (_exportTypes.Any(t => t != null && t.GetKey().Equals(et.GetKey())))
                return;

            _exportTypes.Add(et);
        }

        private void ExportReportTo(ExportType format)
        {
            if (format == null)
                throw new ArgumentNullException("format");

            using (var export = new ExportFormatFactory().GetExportFormat(format.Format))
            using (var stream = new MemoryStream())
            {
                if (export == null)
                    throw new NullReferenceException("export");

                if (export.SupportsEncoding)
                    export.SetEncoding(format.Encoding);

                if (export.SupportsSpacelife)
                    export.SetSpacelife(format.Spacelife);

                export.Export(_report, stream);
                _exportStreams[format.GetKey()] = new EpsStreamType(format.GetKey(), stream.ToArray());
            }
        }

        /// <summary>
        /// Добавление stream-ов по типам для экпорта.
        /// </summary>
        public void DoExport()
        {
            _exportStreams.Clear();
            foreach (var type in _exportTypes)
            {
                ExportReportTo(type);
            }
        }

        /// <summary>
        /// Получение массива данных по имени типа экспорта отчета.
        /// </summary>
        /// <param name="format">имя типа</param>
        /// <returns>массив данных</returns>
        public byte[] GetExportStream(ExportType et)
        {
            if (et == null)
                throw new ArgumentNullException("et");

            return _exportStreams.ContainsKey(et.GetKey()) ? _exportStreams[et.GetKey()].Bytes : null;
        }

        /// <summary>
        /// Печать отчета.
        /// </summary>
        /// <param name="printName">имя принтера</param>
        /// <param name="copies">количество копий</param>
        public void Print(string printName, short copies)
        {
            if (string.IsNullOrEmpty(printName))
                throw new ArgumentNullException("printName");

            //            while (_report.IsPrinting)
            //            {
            //                Thread.Sleep(100);
            //            }
            _report.PrintSettings.ShowDialog = false;
            _report.PrintSettings.Copies = copies;
            _report.PrintSettings.Printer = printName;
            _report.ReportInfo.Name = DateTime.Now.ToLongTimeString() + ":" + _report.FileName;

            if (copies > 0)
            {
                _report.PrintPrepared();
            }
        }

        public void Dispose()
        {
            if (_exportTypes != null)
            {
                _exportTypes.Clear();
            }
            if (_exportStreams != null)
            {
                _exportStreams.Clear();
            }
            if (_report != null)
            {
                _report.Dispose();
            }
        }
    }
}