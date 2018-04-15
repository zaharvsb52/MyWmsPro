using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using log4net;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Managers
{
    public static class EpsJobExecutorHelper
    {
        #region .  Consts & Fields  .
        private const string CpvCheckRoot = "EPSJOBPrepareL1";
        private const string CpvSplitSQL = "EPSJOBSplitParamSQLL2";
        private const string CpvCheckSQL = "EPSJOBCheckSQLL2";
        private const string EpsLogicalPrinter = "EpsLogicalPrinter";


        //Используется для отладки
        //private static object _lock = new object();
        private static readonly ILog _log = LogManager.GetLogger(typeof(EpsJobExecutorHelper)); 
        #endregion

        public static bool RunJob(string jobCode, string executor)
        {
            //Формируем задание для EPS
            EpsJob epsJob;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<EpsJob>>())
                epsJob = mgr.Get(jobCode);

            if (epsJob == null)
                throw new DeveloperException("Cant't find record in EpsJob by key '{0}'.", jobCode);

            // проверяем epsjob. могут вернуться доп. параметры. кол-во строк доп параметров = кол-ву задач, которые нужно запустить
            DataTable paramsTable;
            if (!CheckJob(epsJob, out paramsTable))
                return true;

            // если есть дополнительные параметры - повторяем столько раз, сколько их придет
            if (paramsTable != null)
            {
                foreach (DataRow row in paramsTable.Rows)
                    ProcessOutput(epsJob, executor, row);
            }
            else
                ProcessOutput(epsJob, executor, null);
            return false;
        }

        private static void ProcessOutput(EpsJob epsJob, string executor, DataRow row)
        {
            try
            {
                var output = new Output
                {
                    Login_R = executor,
                    Host_R = WMSEnvironment.Instance.ClientCode,
                    OutputStatus = OutputStatus.OS_NEW.ToString(),
                    EpsHandler = epsJob.JobHandler
                };

                FillOutputParams(epsJob, executor, output, row);

                using (var mgr = IoC.Instance.Resolve<IBaseManager<Output>>())
                    mgr.Insert(ref output);

                _log.InfoFormat("Create new Output '{0}' from Executor '{1}'.", output.GetKey(), executor);
            }
            catch (Exception ex)
            {
                _log.Error(string.Format("Can't create from Executor '{0}'. {1}", executor, ExceptionHelper.ExceptionToString(ex)), ex);
            }
        }

        private static bool CheckJob(EpsJob epsJob, out DataTable table)
        {
            table = null;

            // если заблокировано - не запускаем
            if (epsJob.JobLocked)
            {
                _log.InfoFormat("Job '{0}' is locked", epsJob.GetKey());
                return false;
            }

            // cpv нет - выходим
            if (epsJob.CustomParamVal == null || epsJob.CustomParamVal.Count == 0)
                return true;

            // проверяем наличие CPV проверки
            var root = epsJob.CustomParamVal.FirstOrDefault(i => CpvCheckRoot.Equals(i.CustomParamCode));
            if (root == null)
                return true;

            _log.DebugFormat("Checking by cpv");

            // получаем основной sql
            var cpvSql = epsJob.CustomParamVal.FirstOrDefault(i => CpvSplitSQL.Equals(i.CustomParamCode));
            if (cpvSql == null || string.IsNullOrEmpty(cpvSql.CPVValue))
                throw new OperationException("Can't find cpv with code '{0}' in cpv", CpvSplitSQL);

            // проверяем нет ли быстрой проверки
            var cpvPreSql = epsJob.CustomParamVal.FirstOrDefault(i => CpvCheckSQL.Equals(i.CustomParamCode));
            if (cpvPreSql != null && !string.IsNullOrEmpty(cpvPreSql.CPVValue))
            {
                DataTable preResult;
                using (var mgr = IoC.Instance.Resolve<IBPProcessManager>())
                    preResult = mgr.ExecuteDataTable(cpvPreSql.CPVValue);

                // если ничего не вернулось - проверка не прошла
                if (preResult == null || preResult.Rows.Count == 0)
                {
                    _log.Info("Prefilter return 0 rows. Don't run main filter. Exit correctly");
                    return false;
                }
            }

            // запускаем основной SQL
            using (var mgr = IoC.Instance.Resolve<IBPProcessManager>())
                table = mgr.ExecuteDataTable(cpvSql.CPVValue);

            // если ничего не вернулось, то данных нет, корректно выходим
            if (table == null || table.Rows.Count == 0)
            {
                _log.DebugFormat("Filter return 0 rows. Exit correctly");
                return false;
            }

            return true;
        }

        private static void FillOutputParams(EpsJob epsJob, string executor, Output output, DataRow row)
        {
            var epsJobId = epsJob.GetKey();

            //Параметры EPS
            var lstReports = new List<Report>();
            if (epsJob.ConfigEps == null)
            {
                _log.DebugFormat("EpsJob.ConfigEps is null in EpsJob '{0}' of Executor '{1}'.", epsJobId, executor);
            }
            else
            {
                if (output.EpsParams == null)
                    output.EpsParams = new WMSBusinessCollection<OutputParam>();

                if (output.ReportParams == null)
                    output.ReportParams = new WMSBusinessCollection<OutputParam>();

                var configs = epsJob.ConfigEps.Where(p => p != null && !string.IsNullOrEmpty(p.EpsConfigParamCode) && !p.EpsConfigLocked);
                foreach (var configEps in configs)
                {
                    var configEpsId = configEps.GetKey();
                    switch (Extensions.To(configEps.EpsConfigParamCode, EpsTaskParams.None))
                    {
                        //case EpsTaskParams.None:
                        //    _log.DebugFormat("Can't convert EpsConfigParamCode '{0}' to enum: EpsTaskParams in ConfigEps '{1}' of EpsJob '{2}' of Executor '{3}'.",
                        //        configEps.EpsConfigParamCode, configEpsId, epsJobId, executor);
                        //    continue;

                            //Параметры отчета
                        case EpsTaskParams.EpsReport:
                            if (!string.IsNullOrEmpty(configEps.EpsConfigValue))
                            {
                                // получаем отчет
                                Report report;

                                // TODO: сделать поддержку маски для вложенных коллекций
                                // формируем шаблон получения отчета
                                //                                var attrEntity = FilterHelper.GetAttrEntity<Report>(
                                //                                    Report.ReportCodePropertyName
                                //                                    , Report.ReportFile_RPropertyName
                                //                                    , Report.ReportLockedPropertyName
                                //                                    , Report.ConfigRepPropertyName
                                //                                    , Report.ReportCopiesPropertyName
                                //                                    );
                                //
                                //                                var configAttrEntity = FilterHelper.GetAttrEntity<ReportCfg>(
                                //                                    ReportCfg.EpsConfigParamCodePropertyName,
                                //                                    ReportCfg.EpsConfigValuePropertyName
                                //
                                //                                    );
                                //
                                //                                var cfgAttr = TypeDescriptor.GetProperties(typeof(Report))
                                //                                    .OfType<DynamicPropertyDescriptor>()
                                //                                    .Single(p => p.Name == Report.ConfigRepPropertyName)
                                //                                    .SourceName;
                                //                                var replaceWith = string.Format("<{0}>{1}</{0}>", cfgAttr, configAttrEntity);
                                //
                                //                                attrEntity = attrEntity.Replace(string.Format("<{0} />", cfgAttr), replaceWith);
                                //
                                //                                // получаем отчет
                                using (var mgr = IoC.Instance.Resolve<IBaseManager<Report>>())
                                    report = mgr.Get(configEps.EpsConfigValue);//, attrEntity);

                                if (report == null)
                                    continue;

                                if (report.ReportLocked)
                                {
                                    _log.WarnFormat("Report '{0}' is locked in ConfigEps '{1}' of EpsJob '{2}' of Executor '{3}'.",
                                        configEps.EpsConfigValue, configEpsId, epsJobId, executor);
                                    continue;
                                }

                                if (string.IsNullOrEmpty(report.ReportFile_R))
                                {
                                    _log.WarnFormat("Undefined report file name in report '{0}' of ConfigEps '{1}' of EpsJob '{2}' of Executor '{3}'.",
                                        configEps.EpsConfigValue, configEpsId, epsJobId, executor);
                                    continue;
                                }

                                //получаем файл отчета
                                ReportFile reportFile;
                                using (var mgr = IoC.Instance.Resolve<IBaseManager<ReportFile>>())
                                    reportFile = ((IReportFileManager)mgr).GetByReportFile(report.ReportFile_R);
                                if (reportFile != null)
                                    output.ReportFileSubfolder_R = reportFile.ReportFileSubfolder;

                                lstReports.Add(report);
                                output.ReportParams.Add(new OutputParam
                                {
                                    OutputParamCode = configEps.EpsConfigParamCode,
                                    OutputParamValue = report.ReportFile_R,
                                    OutputParamType = EpsParamType.REP.ToString()
                                });

                                if (report.ConfigRep == null)
                                {
                                    _log.DebugFormat("Report.ConfigRep is null in report '{0}' of ConfigEps '{1}' of EpsJob '{2}' of Executor '{3}'.",
                                        configEps.EpsConfigValue, configEpsId, epsJobId, executor);
                                }
                                else
                                {
                                    foreach (var configRep in report.ConfigRep.Where(p => p != null && !p.EpsConfigLocked))
                                    {
                                        switch (Extensions.To(configRep.EpsConfigParamCode, EpsTaskParams.None))
                                        {
                                            case EpsTaskParams.ResultReportFile:
                                            case EpsTaskParams.ChangeODBC:
                                                break;

                                            case EpsTaskParams.None: //Variable
                                                if (string.IsNullOrEmpty(configRep.EpsConfigParamCode))
                                                    continue;
                                                break;

                                            default:
                                                _log.DebugFormat("Illegal EpsConfigParamCode '{0}'.", configRep.EpsConfigParamCode);
                                                break;
                                        }

                                        var name = configRep.EpsConfigParamCode;
                                        if (!string.IsNullOrEmpty(name) && name[0] == '{' && name[name.Length - 1] == '}')
                                            name = name.Substring(1, name.Length - 2);

                                        var val = configRep.EpsConfigValue;
                                        // если передали строку, пытаемся из нее взять значение парметра
                                        if (row != null)
                                        {
                                            var col = row.Table.Columns.Cast<DataColumn>().FirstOrDefault(i => Extensions.EqIgnoreCase(i.ColumnName, name));
                                            if (col != null)
                                                val = row[(DataColumn) col] == null ? null : row[(DataColumn) col].ToString();
                                        }

                                        output.ReportParams.Add(new OutputParam
                                        {
                                            OutputParamCode = name,
                                            OutputParamValue = val,
                                            OutputParamSubvalue = report.ReportFile_R,
                                            OutputParamType = EpsParamType.REP.ToString()
                                        });
                                    }
                                }
                            }
                            break;

                        default:
                            output.EpsParams.Add(new OutputParam
                            {
                                OutputParamCode = configEps.EpsConfigParamCode,
                                OutputParamValue = configEps.EpsConfigValue,
                                OutputParamType = EpsParamType.EPS.ToString()
                            });
                            break;
                    }
                }
                if (!output.EpsParams.Any()) output.EpsParams = null;
                if (!output.ReportParams.Any()) output.ReportParams = null;
            }

            //Задачи
            if (epsJob.Job2Task == null)
            {
                _log.DebugFormat("EpsJob.Job2Tas is null in EpsJob '{0}' of Executor '{1}'.", epsJobId, executor);
            }
            else
            {
                if (output.OutputTasks == null)
                    output.OutputTasks = new WMSBusinessCollection<OutputTask>();

                using (var epsTaskManager = IoC.Instance.Resolve<IBaseManager<EpsTask>>())
                using (var printerLogicalManager = IoC.Instance.Resolve<IBaseManager<PrinterLogical>>())
                using (var printerPhysicalManager = IoC.Instance.Resolve<IBaseManager<PrinterPhysical>>())
                    foreach (var job2Task in epsJob.Job2Task.Where(p => p != null && !string.IsNullOrEmpty(p.EpsTask2JobTaskCode)))
                    {
                        PrinterLogical printerLogical = null;
                        var epsTask = epsTaskManager.Get(job2Task.EpsTask2JobTaskCode);
                        if (epsTask == null)
                        {
                            _log.DebugFormat("Can't get EpsTask by key '{0}'.", job2Task.EpsTask2JobTaskCode);
                            continue;
                        }

                        if (epsTask.TaskLocked)
                        {
                            _log.DebugFormat("Task '{0}' is locked.", epsTask.GetKey());
                            continue;
                        }

                        EpsTaskType taskType;
                        try
                        {
                            taskType = ConvertToEpsTaskType(epsTask.TaskType);
                        }
                        catch (Exception ex)
                        {
                            _log.Error(ex.Message, ex);
                            continue;
                        }

                        if (taskType == EpsTaskType.None)
                        {
                            _log.Debug("Undefined EPS Task type.");
                            continue;
                        }

                        var outputTasks = new OutputTask
                        {
                            OutputTaskCode = taskType.ToString(),
                            OutputTaskOrder = job2Task.Task2JobOrder,
                            TaskParams = new WMSBusinessCollection<OutputParam>()
                        };

                        if (epsTask.ConfigTsk != null)
                        {
                            foreach (var configTsk in epsTask.ConfigTsk.Where(p => p != null && !string.IsNullOrEmpty(p.EpsConfigParamCode) && !p.EpsConfigLocked))
                            {
                                var epsConfigParamCode = configTsk.EpsConfigParamCode;
                                var paramCod = Extensions.To(epsConfigParamCode, EpsTaskParams.None);
                                var epsConfigValue = configTsk.EpsConfigValue;

                                switch (paramCod)
                                {
                                    case EpsTaskParams.PhysicalPrinter:
                                        continue;

                                    case EpsTaskParams.None:
                                        if (Extensions.EqIgnoreCase(epsConfigParamCode, EpsLogicalPrinter, true))
                                        {
                                            //Задан логический принтер
                                            if (!string.IsNullOrEmpty(epsConfigValue))
                                            {
                                                printerLogical = printerLogicalManager.Get(epsConfigValue);
                                                if (printerLogical == null)
                                                {
                                                    _log.DebugFormat("Can't get PrinterLogical by key '{0}'.", configTsk.EpsConfigValue);
                                                    continue;
                                                }

                                                //Проверяем физический принтер
                                                var printerPhysical = printerPhysicalManager.Get(printerLogical.PhysicalPrinter_R);
                                                if (printerPhysical == null)
                                                {
                                                    var message = string.Format("Can't get PrinterPhysical by key '{0}'.", printerLogical.PhysicalPrinter_R);
                                                    _log.Debug(message);
                                                    continue;
                                                }

                                                if (printerPhysical.PhysicalPrinterLocked)
                                                {
                                                    _log.DebugFormat("Physical printer '{0}' is locked.", printerPhysical.GetKey());
                                                    continue;
                                                }

                                                epsConfigParamCode = EpsTaskParams.PhysicalPrinter.ToString();
                                                epsConfigValue = printerLogical.PhysicalPrinter_R;
                                            }
                                        }
                                        else
                                        {
                                            _log.DebugFormat("Can't convert EpsConfigParamCode '{0}' to enum: EpsTaskParams.", configTsk.EpsConfigParamCode);
                                            continue;
                                        }
                                        break;
                                }

                                outputTasks.TaskParams.Add(new OutputParam
                                {
                                    OutputParamCode = epsConfigParamCode,
                                    OutputParamValue = epsConfigValue,
                                    OutputParamType = EpsParamType.TSK.ToString()
                                });
                            }

                            //Проверяем наличие физ. принтера
                            if (taskType == EpsTaskType.OTC_PRINT)
                            {
                                if (!outputTasks.TaskParams.Any(p => p != null && Extensions.To(p.OutputParamCode, EpsTaskParams.None) == EpsTaskParams.PhysicalPrinter))
                                    _log.DebugFormat("PhysicalPrinter parameter is not present for task '{0}'.", job2Task.GetKey());

                                //Добавляем copies для задачи Print
                                if (lstReports != null)
                                {
                                    var copies = outputTasks.TaskParams.Where(p => p != null && Extensions.To(p.OutputParamCode, EpsTaskParams.None) == EpsTaskParams.Copies).ToArray();
                                    if (copies.Any()) //Удаляем
                                    {
                                        foreach (var p in copies)
                                        {
                                            outputTasks.TaskParams.Remove(p);
                                        }

                                        //Определяем новые copies с учетом отчета и принтера
                                        var copy = copies.Select(p => Extensions.To(p.OutputParamValue, 0)).First();
                                        foreach (var report in lstReports.Where(p => p != null && !string.IsNullOrEmpty(p.ReportFile_R)))
                                        {
                                            outputTasks.TaskParams.Add(new OutputParam
                                            {
                                                OutputParamCode = EpsTaskParams.Copies.ToString(),
                                                OutputParamValue = (report.ReportCopies * copy * (printerLogical == null ? 0 : printerLogical.LogicalPrinterCopies)).ToString(CultureInfo.InvariantCulture),
                                                OutputParamSubvalue = report.ReportFile_R,
                                                OutputParamType = EpsParamType.TSK.ToString()
                                            });
                                        }
                                    }
                                }
                            }
                        }
                        if (outputTasks.TaskParams.Count == 0)
                            outputTasks.TaskParams = null;

                        output.OutputTasks.Add(outputTasks);
                    }

                if (output.OutputTasks.Count == 0)
                    output.OutputTasks = null;
            }
        }

        private static EpsTaskType ConvertToEpsTaskType(string taskType)
        {
            var taskTypeinternal = taskType.GetTrim();
            if (string.IsNullOrEmpty(taskTypeinternal))
                return EpsTaskType.None;

            switch (taskTypeinternal.ToUpper())
            {
                case "DCL":
                    return EpsTaskType.OTC_DCL;
                case "FTP":
                    return EpsTaskType.OTC_FTP;
                case "MAIL":
                    return EpsTaskType.OTC_MAIL;
                case "PRINT":
                    return EpsTaskType.OTC_PRINT;
                case "SHARE":
                    return EpsTaskType.OTC_SHARE;
                default:
                    throw new DeveloperException(string.Format("Undefined type of task: '{0}'.", taskType));
            }
        }
    }
}