#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="wmsEPSJob.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Vladimir G. Nosov</Author>
/// <Date>20.09.2012 8:20:32</Date>
/// <Summary>Задание сервиса EPS</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using log4net;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.EPS.wmsEPS.Helpers;
using wmsMLC.General;

namespace wmsMLC.EPS.wmsEPS
{
    /// <summary>
    /// Задание сервиса EPS.
    /// </summary>
    public class EpsJob : IDisposable
    {
        private static ILog _logger = LogManager.GetLogger(typeof(EpsJob));

        /// <summary>
        /// Объект БД.
        /// </summary>
        private readonly Output _output;

        /// <summary>
        /// //Коллекция отчетов, ключ - файл отчета.
        /// </summary>
        private Dictionary<string, EpsFastReport> _reports;

        public Dictionary<string, EpsFastReport> Reports
        {
            get { return _reports; }
        }

        /// <summary>
        /// Список задач в задании.
        /// </summary>
        public List<EpsTask> Tasks
        {
            get { return _tasks; }
        }

        private bool _zip;

        private bool _reserve;

        private string _jobFolder = string.Empty;

        /// <summary>
        /// Список задач в задании.
        /// </summary>
        private readonly List<EpsTask> _tasks;

        private void CreateReports(string userName)
        {
            //проверяем есть ли параметры задающие файлы отчета?
            if (_output.ReportParams == null || !_output.ReportParams.Any(p => p != null && p.OutputParamCode.To(EpsTaskParams.None) == EpsTaskParams.EpsReport))
                return;

            //Формируем более "дружественную" коллекцию параметров отчета
            var reportParams = _output.ReportParams.Where(p => p != null)
                .Select(p => new
                {
                    Code = p.OutputParamCode,
                    ParamType = p.OutputParamCode.To(EpsTaskParams.None),
                    Value = p.OutputParamValue,
                    Subvalue = p.OutputParamSubvalue,
                })
                .Distinct()
                .ToArray();

            Func<EpsTaskParams, string, string> getValueHandler = (parType, epsreport) =>
            {
                //1. Проверяем на заполнение поле Subvalue.
                var result = reportParams
                    .Where(p => p.ParamType == parType && p.Subvalue.EqIgnoreCase(epsreport))
                    .Select(p => p.Value)
                    .FirstOrDefault();

                if (result.IsNullOrEmptyAfterTrim())
                {
                    //2. Если не нашли берем параметр, у которого поле Subvalue == null
                    result = reportParams
                        .Where(p => p.ParamType == parType && p.Subvalue == null)
                        .Select(p => p.Value)
                        .FirstOrDefault();
                }

                return result;
            };

            var jobid = _output.GetKey<object>();
            var login = _output.Login_R;

            //Формируем отчеты
            //Группируем по значению поля Value.
            foreach (var rep in reportParams.Where(p => p.ParamType == EpsTaskParams.EpsReport && !p.Value.IsNullOrEmptyAfterTrim()).GroupBy(k => k.Value))
            {
                //получаем параметр типа EpsTaskParams.ResultReportFile
                var resultReportFile = getValueHandler(EpsTaskParams.ResultReportFile, rep.Key);
                if (resultReportFile.IsNullOrEmptyAfterTrim())
                {
                    //TODO: возможно берем еще откуда-нибудь.
                    resultReportFile = EpsHelper.ReportNameMacro;
                }
                resultReportFile = ProcessReportFileName(resultReportFile, _output.ReportParams);

                //получаем параметр типа EpsTaskParams.ChangeODBC. 0 - строку переопределять (переопределяет все подключения в отчете), 1 - строку не переопределять.
                var changeOdbc = getValueHandler(EpsTaskParams.ChangeODBC, rep.Key).To(0);

                //получим пользовательский параметр ReportUseODAC
                var reportUseOdac = EpsHelper.GetReportCustomParameter(Path.GetFileNameWithoutExtension(rep.Key), ReportCpv.ReportUseOdacParameter);

                var connectionString = changeOdbc == 0 ? Properties.Settings.Default.ConnectionStringOdbc : null;

                //параметр ReportUseODAC в приоритете
                if (reportUseOdac.To(0) == 1)
                    connectionString = Properties.Settings.Default.ConnectionStringOdac;

                //инициализация отчета
                var reportPath = (_output.ReportFileSubfolder_R != null) ? Path.Combine(Properties.Settings.Default.ReportPath, _output.ReportFileSubfolder_R, rep.Key) : Path.Combine(Properties.Settings.Default.ReportPath, rep.Key);

                var report = new EpsFastReport(
                    EpsHelper.SetFileName(resultReportFile, Path.GetFileNameWithoutExtension(rep.Key)),
                    reportPath,
                    connectionString,
                    Properties.Settings.Default.ArchivePath + "\\frtmp"
                    );

                _reports[rep.Key] = report;

                //получаем параметры типа EpsTaskParams.Variable
                //это как должно быть
                var variables = reportParams.Where(p => p.ParamType == EpsTaskParams.None && rep.Key.EqIgnoreCase(p.Subvalue));
                foreach (var variable in variables.Where(p => p != null && !p.Value.IsNullOrEmptyAfterTrim()))
                {
                    report.AddParameters(variable.Code, variable.Value);
                }

                //Добавляем в отчет параметр UserCode
                report.AddParameters("UserCode", userName);

                _logger.Info(string.Format("Job = '{0}' ('{1}'), EpsReport = '{2}', ResultReportFile = '{3}'.", jobid, login, report.ReportName, resultReportFile));
            }
        }

        private void ParseJobParams()
        {
            if (_output.EpsParams == null)
                return;

            foreach (var p in _output.EpsParams.Where(p => p != null))
            {
                switch (p.OutputParamCode.To(EpsTaskParams.None))
                {
                    case EpsTaskParams.Zip:
                        _zip = true;
                        break;
                    case EpsTaskParams.ReserveCopy:
                        _reserve = true;
                        break;
                }
            }
        }

        private void CreateJobFolder()
        {
            var currentDate = DateTime.Now;
            _jobFolder = Path.Combine(Properties.Settings.Default.ArchivePath, currentDate.Year.ToString(CultureInfo.InvariantCulture), currentDate.Month.ToString(CultureInfo.InvariantCulture), currentDate.Day.ToString(CultureInfo.InvariantCulture), EpsHelper.GetKey(_output).ToString());

            if (!_reserve) 
                return;

            try
            {
                if (!Directory.Exists(_jobFolder))
                {
                    Directory.CreateDirectory(_jobFolder);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(new DeveloperException(string.Format("Can't crate folder '{0}'.", _jobFolder), ex));
                _jobFolder = Properties.Settings.Default.ArchivePath;
            }
        }

        /// <summary>
        /// Конструктор задания.
        /// </summary>
        public EpsJob(Output output, string userName)
        {
            if (output == null)
                throw new ArgumentNullException("output");

            _output = output;
            _zip = false;
            _reserve = false;
            _tasks = new List<EpsTask>();
            _reports = new Dictionary<string, EpsFastReport>();

            try
            {
                CreateReports(userName);
                ParseJobParams();
                CreateJobFolder();

                if (_output.OutputTasks == null || _output.OutputTasks.Count == 0)
                    throw new Exception("Output tasks are empty.");

                //сортируем в соответствии с OutputTaskOrder
                foreach (var obj in _output.OutputTasks.Where(p => p != null).OrderBy(p => p.OutputTaskOrder))
                {
                    var task = new EpsTask(obj, EpsHelper.GetKey(_output).To<decimal>(), _jobFolder, _zip, _reserve);
                    Tasks.Add(task);

                    if (_reports == null)
                        continue;

                    if (task.ExportType != null)
                    {
                        foreach (var p in _reports.Select(p => p.Value).Where(p => p != null))
                        {
                            p.AddExportType(task.ExportType);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _output.OutputStatus = EpsJobStatus.OS_ERROR.ToString();
                _output.OutputFeedback = ex.Message;
                throw;
            }
        }

        private string ProcessReportFileName(string resultReportFile, IEnumerable<OutputParam> parameters)
        {
            const string sqlMacroSuf = "${SQL=";

            if (string.IsNullOrEmpty(resultReportFile))
                return resultReportFile;

            if (!resultReportFile.StartsWith(sqlMacroSuf))
                return resultReportFile;

            try
            {
                // вычленяем запрос
                var sql = resultReportFile.Replace(sqlMacroSuf, string.Empty);
                sql = sql.Substring(0, sql.Length - 1);

                foreach (var param in parameters)
                {
                    var paramName = param.OutputParamCode[0] == '{' ? param.OutputParamCode : "{" + param.OutputParamCode + "}";
                    sql = sql.Replace(paramName, param.OutputParamValue);
                }

                using (var mgr = IoC.Instance.Resolve<IBPProcessManager>())
                {
                    var table = mgr.ExecuteDataTable(sql);
                    if (table == null || table.Rows.Count == 0)
                        throw new DeveloperException("Вернулось 0 строк данных.");

                    if (table.Rows[0].IsNull(0))
                        throw new DeveloperException("Вернулось пустое значение.");

                    var name = Convert.ToString(table.Rows[0][0], CultureInfo.InvariantCulture);
                    if (string.IsNullOrEmpty(name))
                        throw new DeveloperException("Вернулась пустая строка.");

                    // текст может быть в ковычках
                    return name.Replace("'", string.Empty);
                }
            }
            catch (Exception ex)
            {
                var message = string.Format("Ошибка получения имени файла по макросу {0}. {1}", resultReportFile, ex.Message);
                throw new DeveloperException(message, ex);
            }
        }

        /// <summary>
        /// Запуск задания.
        /// </summary>
        public void DoJob()
        {
            var watch = new Stopwatch();
            watch.Start();
            try
            {
                if (_output.OutputStatus.To(EpsJobStatus.OS_COMPLETED) == EpsJobStatus.OS_ERROR)
                    return;

                EpsFastReport[] reports = null;

                //TODO: получение экспорта форматов отчета 
                if (_reports != null)
                {
                    reports = _reports.Select(p => p.Value).Where(p => p != null).ToArray();
                    foreach (var p in reports)
                    {
                        p.Prepare();
                        p.DoExport();
                    }
                }

                foreach (var taskGroup in Tasks.GroupBy(x => x.OutputTask.OutputTaskOrder).OrderBy(g => g.Key))
                {
                    var tasks =
                        taskGroup.Select(epsTask => Task.Factory.StartNew(() => epsTask.Start(reports))).ToArray();
                    Task.WaitAll(tasks);
                }

                _output.OutputStatus = EpsJobStatus.OS_COMPLETED.ToString();
            }
            catch (AggregateException ae)
            {
                var e = ae.Flatten();
                var sb = new StringBuilder();
                foreach (var ie in e.InnerExceptions)
                {
                    if (sb.Length > 0)
                        sb.Append(", ");
                    sb.Append(ie.Message);
                }
                _output.OutputStatus = EpsJobStatus.OS_ERROR.ToString();
                _output.OutputFeedback = sb.ToString();
                throw new Exception(sb.ToString());
            }
            catch (Exception ex)
            {
                _output.OutputStatus = EpsJobStatus.OS_ERROR.ToString();
                _output.OutputFeedback = ex.Message;
                throw;
            }
            finally
            {
                watch.Stop();
                _output.OutputTime = watch.Elapsed.ToString();
            }
        }

        /// <summary>
        /// Получение объекта задания.
        /// </summary>
        /// <returns>объект задания</returns>
        public Output GetOutput()
        {
            return _output;
        }

        public void Dispose()
        {
            if (Tasks != null)
            {
                foreach (var epsTask in Tasks)
                    epsTask.Dispose();
            }

            if (_reports == null) 
                return;
            foreach (var p in _reports.Select(p => p.Value).Where(p => p != null))
            {
                try
                {
                    p.Dispose();
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch { }
                // ReSharper restore EmptyGeneralCatchClause
            }
            _reports = null;
        }
    }
}
