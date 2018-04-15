#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="wmsEPSTask.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Vladimir G. Nosov</Author>
/// <Date>20.09.2012 8:20:38</Date>
/// <Summary>Задача сервиса EPS</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Text;
using wmsMLC.Business.Objects;
using wmsMLC.EPS.wmsEPS.OutputTasks;
using wmsMLC.General;

namespace wmsMLC.EPS.wmsEPS
{
    /// <summary>
    /// Задача сервиса EPS
    /// </summary>
    public class EpsTask : IDisposable
    {
        /// <summary>
        /// Объект задачи в БД.
        /// </summary>
        //private TOBJEPSOUTPUTTASK _task;
        private readonly OutputTask _task;
        public OutputTask OutputTask
        {
            get { return _task; }
        }

        /// <summary>
        /// Тип задачи.
        /// </summary>
        private readonly EpsTaskType _taskType;

        /// <summary>
        /// Номер задания.
        /// </summary>
        private readonly decimal _jobId;

        /// <summary>
        /// Сжимать файл
        /// </summary>
        private readonly bool _zip;

        /// <summary>
        /// Делать резервную копию
        /// </summary>
        private readonly bool _reserve;

        private readonly string _workingFolder;

        private readonly ExportType _exportType;

        public ExportType ExportType
        {
            get { return _exportType; }
        }

        // Интерфейс OTC_Like задач
        private IEpsOutputTask _iTask;

        /// <summary>
        /// Конструктор задачи.
        /// </summary>
        public EpsTask(OutputTask task, decimal jobId, string workingFolder, bool zip, bool reserve)
        {
            _task = task;
            _jobId = jobId;
            _workingFolder = workingFolder;
            _zip = zip;
            _reserve = reserve;

            _taskType = _task.OutputTaskCode.To(EpsTaskType.None);
            if (_taskType == EpsTaskType.None)
                throw new ArgumentException(string.Format("Undefined OutputTaskCode '{0}' in OutputTask.", _task.OutputTaskCode));

            if (task.TaskParams == null) return;
            
            var exportFormat = task.TaskParams.Where(
                p => p != null && p.OutputParamCode.To(EpsTaskParams.None) == EpsTaskParams.FileFormat)
                .Select(p => p.OutputParamValue)
                .FirstOrDefault(); //больше не должно быть форматов

            var encodingName =
                task.TaskParams.Where(
                    p => p != null && p.OutputParamCode.To(EpsTaskParams.None) == EpsTaskParams.Conversion)
                    .Select(p => p.OutputParamValue)
                    .FirstOrDefault();
            var encoding = string.IsNullOrEmpty(encodingName) ? Encoding.UTF8 : Encoding.GetEncoding(encodingName);

            var spacelife =
                task.TaskParams.Any(
                    p => p != null 
                        && p.OutputParamCode.To(EpsTaskParams.None) == EpsTaskParams.Spacelife
                        && p.OutputParamValue.To(0) != 0);

            _exportType = !string.IsNullOrEmpty(exportFormat) ? new ExportType { Encoding = encoding, Format = exportFormat, Spacelife = spacelife } : null;
        }


        /// <summary>
        /// Запуск задачи.
        /// </summary>
        public void Start(EpsFastReport[] reports)
        {
            var sw = new Stopwatch();
            sw.Start();

            try
            {
                _iTask = new OTCFactory().Create(_taskType);
                if (_task.TaskParams != null)
                {
                    foreach (var p in _task.TaskParams.Where(p => p != null))
                    {
                        _iTask.AddParameter(p.OutputParamCode, p.OutputParamValue, p.OutputParamSubvalue);
                    }
                }
                _iTask.SetExportType(ExportType);
                _iTask.Zip(_zip, _reserve);
                _iTask.DoTask(reports);

                // Создание резервных копий файлов задач
                if (_reserve)
                {
                    var files = _iTask.GetFiles();
                    if (files != null)
                    {
                        foreach (var fs in files)
                        {
                            var fileName = Path.Combine(_workingFolder, _jobId + "_" + fs.FileName);
                            if (!File.Exists(fileName))
                            {
                                File.WriteAllBytes(fileName, fs.Data);
                            }
                        }
                    }
                }
                _task.OutputTaskFeedback = string.Empty;
                _task.OutputTaskStatus = EpsJobStatus.OS_COMPLETED.ToString();
            }
            catch (Exception ex)
            {
                _task.OutputTaskStatus = EpsJobStatus.OS_ERROR.ToString();
                _task.OutputTaskFeedback = ex.Message;
                throw;
            }
            finally
            {
                sw.Stop();
                _task.OutputTaskTime = sw.Elapsed.ToString();
            }
        }

        public void Dispose()
        {
            var dispTask = _iTask as IDisposable;
            if (dispTask != null)
                dispTask.Dispose();
        }
    }
}
