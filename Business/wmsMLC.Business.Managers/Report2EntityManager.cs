using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using wmsMLC.Business.DAL;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Managers
{
    public class Report2EntityManager : WMSBusinessObjectManager<Report2Entity, decimal>, IReport2EntityManager
    {
        private readonly Regex _regex = new Regex(@"\{\s*(.[^\}]*)\s*\}", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        public IEnumerable<Report2Entity> GetReportsForEntity(Type entityType)
        {
            var entityName = SourceNameHelper.Instance.GetSourceName(entityType);
            var propertyName = SourceNameHelper.Instance.GetPropertySourceName(typeof(Report2Entity), Report2Entity.Report2EntityObjectNamePropertyName);
            var filter = string.Format("{0} = '{1}'", propertyName, entityName.ToUpper());
            return GetFiltered(filter, GetModeEnum.Partial);
        }

        public IEnumerable<Report> GetReports(Type entityType)
        {
            var linkList = GetReportsForEntity(entityType);
            // получаем список самих отчетов
            using (var reportManager = GetManager<Report>())
                return linkList.Select(i => reportManager.Get(i.Report2EntityReport)).ToArray();
            //Результат фильтруем сами
        }

        private void CheckParam(Output output, WMSBusinessObject entity, Report report)
        {
            var entitypds = TypeDescriptor.GetProperties(entity);

            var parameters = report.ConfigRep.Where(p => p != null && !p.EpsConfigLocked);
            foreach (var configRep in parameters)
            {
                var epsConfigParamCode = configRep.EpsConfigParamCode;
                string str = null;
                if (epsConfigParamCode.To(EpsTaskParams.None) == EpsTaskParams.None)
                {
                    if (string.IsNullOrEmpty(epsConfigParamCode))
                        continue;

                    //Variable
                    //Если наименование параметра отчета имеет формат {Параметр}, то в значение такого параметра подставляем значение свойства сущности если есть.
                    var expressions = _regex.Matches(epsConfigParamCode);
                    if (expressions.Count > 0)
                    {
                        var parametername = expressions[0].Groups[1].Value;
                        var entityprop = entitypds.Find(parametername, true);
                        if (entityprop != null)
                        {
                            var value = entityprop.GetValue(entity);
                            str = value == null ? null : value.To<string>();
                            epsConfigParamCode = parametername; //убираем {} из названия параметра
                        }
                    }
                }

                output.ReportParams.Add(new OutputParam
                    {
                        OutputParamCode = epsConfigParamCode,
                        OutputParamValue = str,
                        OutputParamSubvalue = report.ReportFile_R,
                        OutputParamType = EpsParamType.REP.ToString()
                    });
            }
        }

        public Stream ExpReport(string reportCode,
                                           string fileName,
                                           string path,
                                           WMSBusinessObject entity,
                                           IEnumerable<OutputParam> paramExt = null)
        {
            if (string.IsNullOrEmpty(reportCode))
                throw new ArgumentNullException("reportCode");
            Report report;
            using (var reportManager = GetManager<Report>())
                report = reportManager.Get(reportCode);

            if (report.ReportLocked)
                return null;
            //return new PrintReportStatus(wmsMLC.General.Resources.StringResources.FormatReportIsLocked,
            //                             report.ReportName);

            //получаем файл отчета
            ReportFile reportFile;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<ReportFile>>())
                reportFile = ((IReportFileManager)mgr).GetByReportFile(report.ReportFile_R);

            //Формируем задание для EPS
            Output output;
            var outputManager = (IOutputManager)GetManager<Output>();

            output = outputManager.New();
            output.Login_R = WMSEnvironment.Instance.AuthenticatedUser == null
                ? null
                : WMSEnvironment.Instance.AuthenticatedUser.GetSignature();
            output.Host_R = WMSEnvironment.Instance.ClientCode;
            output.OutputStatus = OutputStatus.OS_NEW.ToString();
            output.EpsHandler = report.EpsHandler;
            if (reportFile != null)
                output.ReportFileSubfolder_R = reportFile.ReportFileSubfolder;

            //Параметры отчета
            if (output.ReportParams == null)
                output.ReportParams = new WMSBusinessCollection<OutputParam>();

            output.ReportParams.Add(new OutputParam
            {
                OutputParamCode = EpsTaskParams.ResultReportFile.ToString(),
                OutputParamValue = Path.GetFileNameWithoutExtension(fileName),
                OutputParamSubvalue = report.ReportFile_R,
                OutputParamType = EpsParamType.REP.ToString()
            });

            output.ReportParams.Add(new OutputParam
            {
                OutputParamCode = EpsTaskParams.EpsReport.ToString(),
                OutputParamValue = report.ReportFile_R,
                OutputParamType = EpsParamType.REP.ToString()
            });

            if (report.ConfigRep != null)
            {
                if (entity == null)
                    foreach (var configRep in report.ConfigRep.Where(p => p != null && !p.EpsConfigLocked))
                    {
                        output.ReportParams.Add(new OutputParam
                        {
                            OutputParamCode = configRep.EpsConfigParamCode,
                            OutputParamValue = configRep.EpsConfigValue,
                            OutputParamSubvalue = report.ReportFile_R,
                            OutputParamType = EpsParamType.REP.ToString()
                        });
                    }
                else
                    CheckParam(output, entity, report);
            }

            // заполним доп. парамтеры отчета
            if (paramExt != null)
            {
                foreach (var p in paramExt)
                    output.ReportParams.Add(p);
            }

            var outputTask = new OutputTask
            {
                //OutputTaskCode = EpsTaskType.OTC_SHARE.ToString(),
                OutputTaskCode = EpsTaskType.OTC_DCL.ToString(),
                OutputTaskOrder = 0,
                TaskParams = new WMSBusinessCollection<OutputParam>()
            };
            outputTask.TaskParams.Add(new OutputParam
            {
                OutputParamCode = EpsTaskParams.FileFormat.ToString(),
                OutputParamValue = "wmsMLC.EPS.wmsEPS.ExportTypes.FRFpx",
                OutputParamType = EpsParamType.TSK.ToString()
            });
            outputTask.TaskParams.Add(new OutputParam
            {
                OutputParamCode = EpsTaskParams.TargetFolder.ToString(),
                OutputParamValue = path,
                OutputParamType = EpsParamType.TSK.ToString()
            });
            outputTask.TaskParams.Add(new OutputParam
            {
                OutputParamCode = EpsTaskParams.CopyFile.ToString(),
                OutputParamValue = "1",
                OutputParamType = EpsParamType.TSK.ToString()
            });

            output.OutputTasks = new WMSBusinessCollection<OutputTask> { outputTask };

            return outputManager.GetReportPreview(output);

            //return new PrintReportStatus { Job = output.GetKey().ToString() };
        }

        private static decimal? GetMandantCode(WMSBusinessObject entity)
        {
            //            decimal? mandantCode = null;
            //            if (entity != null)
            //            {
            //                //Поиск манданта
            //                var mto = IoC.Instance.Resolve<IManagerForObject>();
            //                var mgrType = mto.GetManagerByTypeName(entity.GetType().Name);
            //                var managerInstance = IoC.Instance.Resolve(mgrType, null) as IMandantHandler;
            //                mandantCode = managerInstance == null ? null : managerInstance.GetMandantCode(entity);
            //            }
            //            return mandantCode;
            return BPH.GetMandantId(entity);
        }

        public PrintReportStatus PrintReport(decimal? mandantId,
                                             string reportCode,
                                             string printerCode = null,
                                             IEnumerable<OutputParam> paramExt = null)
        {
            PrinterLogical printerLogical;
            var output = CreateOutput(null, reportCode, mandantId, printerCode, paramExt, out printerLogical);

            using (var outputManager = (IOutputManager)GetManager<Output>())
            {
                var result = new PrintReportStatus();
                try
                {
                    if (printerLogical == null)
                        throw new OperationException("Не удалось определить логический принтер");
                    result.Printer = printerLogical.PhysicalPrinter_R;
                    Output outTask = outputManager.PrintReport(output);
                    result.Job = outTask.GetKey().ToString();
                }
                catch (Exception ex)
                {
                    result.Error = ex.Message;
                }
                return result;
            }
        }

        public PrintReportStatus PrintReport(WMSBusinessObject entity,
                                             string reportCode,
                                             string printerCode = null,
                                             IEnumerable<OutputParam> paramExt = null)
        {
            decimal? mandantcode = GetMandantCode(entity);
            PrinterLogical printerLogical;
            var output = CreateOutput(entity, reportCode, mandantcode, printerCode, paramExt, out printerLogical);

            if (printerLogical == null)
                throw new DeveloperException("Не настроен логический принтер");

            using (var outputManager = (IOutputManager)GetManager<Output>())
            {
                var result = new PrintReportStatus { Printer = printerLogical.PhysicalPrinter_R };
                try
                {
                    var outTask = outputManager.PrintReport(output);
                    result.Job = outTask.GetKey().ToString();
                }
                catch (Exception ex)
                {
                    result.Error = ex.Message;
                }
                return result;
            }
        }

        public PrintReportStatus PrintReportBatch(WMSBusinessObject[] entities,
                                             string reportCode,
                                             string printerCode = null,
                                             IDictionary<WMSBusinessObject, IEnumerable<OutputParam>> paramExt = null)
        {
            var outputs = new List<Output>();
            var printers = new List<string>();
            var result = new PrintReportStatus();

            foreach (var entity in entities)
            {
                decimal? mandantcode = GetMandantCode(entity);
                PrinterLogical printerLogical;
                var output = CreateOutput(entity, reportCode, mandantcode, printerCode, paramExt.ContainsKey(entity) ? paramExt[entity] : null, out printerLogical);
                outputs.Add(output);

                if (!printers.Contains(printerLogical.PhysicalPrinter_R))
                    printers.Add(printerLogical.PhysicalPrinter_R);
            }

            using (var outputManager = (IOutputManager)GetManager<Output>())
            {
                try
                {
                    var batch = new OutputBatch { Batch = new WMSBusinessCollection<Output>(outputs) };
                    batch = outputManager.PrintReportBatch(batch);
                    result.Printer = String.Join(", ", printers);
                    result.Job = "-";
                }
                catch (Exception ex)
                {
                    result.Error = ex.Message;
                }
            }
            return result;
        }

        private Output CreateOutput(WMSBusinessObject entity, string reportCode, decimal? mandantcode, string printerCode,
            IEnumerable<OutputParam> paramExt, out PrinterLogical printerLogical)
        {
            if (string.IsNullOrEmpty(reportCode))
                throw new ArgumentNullException("reportCode");

            Output output = null;
            printerLogical = null;

            //Формируем задание для EPS
            using (var outputManager = GetManager<Output>())
                output = outputManager.New();
            output.Login_R = WMSEnvironment.Instance.AuthenticatedUser == null ? null : WMSEnvironment.Instance.AuthenticatedUser.GetSignature();
            output.Host_R = WMSEnvironment.Instance.ClientCode;
            output.OutputStatus = OutputStatus.OS_NEW.ToString();

            //Определяем отчет
            //Используем вариант 2: RR -> SC
            var reportCodeInternal = reportCode;
            //1. RR
            var reportRedefinition = GetDefaultReport(entity, output.Host_R, reportCode, mandantcode);
            decimal reportRedefinitionCopies = 1;
            if (reportRedefinition != null && !reportRedefinition.ReportRedefinitionLocked && !string.IsNullOrEmpty(reportRedefinition.ReportRedefinitionReport))
            {
                reportCodeInternal = reportRedefinition.ReportRedefinitionReport;
                reportRedefinitionCopies = reportRedefinition.ReportRedefinitionCopies;
            }

            Report report;
            using (var reportManager = GetManager<Report>())
                report = reportManager.Get(reportCodeInternal);
            if (report == null)
                return output;
            if (report.ReportLocked)
                return output;
            output.EpsHandler = report.EpsHandler;

            //получаем файл отчета
            ReportFile reportFile;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<ReportFile>>())
                reportFile = ((IReportFileManager)mgr).GetByReportFile(report.ReportFile_R);
            if (reportFile != null)
                output.ReportFileSubfolder_R = reportFile.ReportFileSubfolder;

            //Параметры отчета
            if (output.ReportParams == null)
                output.ReportParams = new WMSBusinessCollection<OutputParam>();

            output.ReportParams.Add(new OutputParam
            {
                OutputParamCode = EpsTaskParams.EpsReport.ToString(),
                OutputParamValue = report.ReportFile_R,
                OutputParamType = EpsParamType.REP.ToString()
            });

            if (report.ConfigRep != null && entity != null)
                CheckParam(output, entity, report);

            if (paramExt != null)
            {
                foreach (var p in paramExt)
                {
                    output.ReportParams.Add(new OutputParam
                    {
                        OutputParamCode = p.OutputParamCode,
                        OutputParamValue = p.OutputParamValue,
                        OutputParamSubvalue = report.ReportFile_R,
                        OutputParamType = EpsParamType.REP.ToString()
                    });
                }
            }

            //Задача на печать
            if (output.OutputTasks == null)
                output.OutputTasks = new WMSBusinessCollection<OutputTask>();

            var outputTasks = new OutputTask
            {
                OutputTaskCode = EpsTaskType.OTC_PRINT.ToString(),
                OutputTaskOrder = 1,
                TaskParams = new WMSBusinessCollection<OutputParam>()
            };

            //Определяем принтер
            //2. SC
            //Если ШК задан, то ищем логический принтер по ШК
            decimal printStreamConfigCopies = 1;
            using (var printerLogicalManager = GetManager<PrinterLogical>())
            {
                if (!string.IsNullOrEmpty(printerCode))
                {
                    var filter = string.Format("({1} = '{0}' OR {2} = '{0}') AND {3} = 0",
                        printerCode,
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(PrinterLogical), PrinterLogical.LOGICALPRINTERPropertyName).ToUpper(),
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(PrinterLogical), PrinterLogical.LogicalPrinterBarCodePropertyName).ToUpper(),
                        SourceNameHelper.Instance.GetPropertySourceName(typeof(PrinterLogical), PrinterLogical.LogicalPrinterLockedPropertyName).ToUpper());

                    var printers = printerLogicalManager.GetFiltered(filter, GetModeEnum.Partial).ToArray();
                    if (printers.Length > 1)
                        throw new OperationException("По коду '{0}' найдено более одного принтера: {1}", printerCode, string.Join(",", printers.Select(i => i.GetKey())));
                    if (printers.Length == 1)
                        printerLogical = printers[0];
                }

                //Если ШК задан и нашли логический принтер по ШК, то не используем SC                
                if (printerLogical == null)
                {
                    var printStreamConfig = GetDefaultPrinter(output.Host_R, output.Login_R, reportCodeInternal, mandantcode);
                    if (printStreamConfig == null)
                        return output;

                    if (printStreamConfig.PrintStreamLocked)
                        return output;

                    if (string.IsNullOrEmpty(printStreamConfig.LogicalPrinter_R))
                        return output;

                    printStreamConfigCopies = printStreamConfig.PrintStreamCopies;
                    printerLogical = printerLogicalManager.Get(printStreamConfig.LogicalPrinter_R, GetModeEnum.Partial);
                }
            }

            if (printerLogical == null)
                return output;

            if (printerLogical.LogicalPrinterLocked)
                return output;

            //Проверяем физический принтер
            PrinterPhysical printerPhysical;
            using (var printerPhysicalManager = GetManager<PrinterPhysical>())
                printerPhysical = printerPhysicalManager.Get(printerLogical.PhysicalPrinter_R, GetModeEnum.Partial);

            if (printerPhysical == null)
                return output;

            if (printerPhysical.PhysicalPrinterLocked)
                return output;

            outputTasks.TaskParams.Add(new OutputParam
            {
                OutputParamCode = EpsTaskParams.PhysicalPrinter.ToString(),
                OutputParamValue = printerLogical.PhysicalPrinter_R,
                OutputParamType = EpsParamType.TSK.ToString()
            });

            decimal copies = report.ReportCopies * reportRedefinitionCopies * printStreamConfigCopies * printerLogical.LogicalPrinterCopies;
            outputTasks.TaskParams.Add(new OutputParam
            {
                OutputParamCode = EpsTaskParams.Copies.ToString(),
                OutputParamValue = copies.ToString(CultureInfo.InvariantCulture),
                OutputParamType = EpsParamType.TSK.ToString()
            });

            output.OutputTasks.Add(outputTasks);
            return output;
        }

        public PrintStreamConfig GetDefaultPrinter(string host, string login, string reportfilename, decimal? mandantcode)
        {
            var type = typeof(PrintStreamConfig);
            var tlist = new List<string>
                    {
                        CreateTparam(PrintStreamConfig.Report_RPropertyName, reportfilename, type),
                        CreateTparam(PrintStreamConfig.Host_RPropertyName, host, type),
                        CreateTparam(PrintStreamConfig.Login_RPropertyName, login, type),
                        CreateTparam(PrintStreamConfig.MandantID_RPropertyName, mandantcode, type),
                        CreateTparam("CHECKTIME", DateTime.Now.ToString(SerializationHelper.DefaultDateTimeStringFormat), null),
                        CreateTparam(PrintStreamConfig.EventKindCode_RPropertyName, null, type),
                        CreateTparam(PrintStreamConfig.FactoryID_RPropertyName, null, type)
                    };

            return GetDefaultPrinter(CreateTListParams(tlist));
        }

        public PrintStreamConfig GetDefaultPrinter(string tListParams)
        {
            using (var repo = GetRepository<IReport2EntityRepository>())
                return repo.GetDefaultPrinter(tListParams);
        }

        public ReportRedefinition GetDefaultReport(WMSBusinessObject entity, string host, string reportfilename, decimal? mandantcode)
        {
            var type = typeof(PrintStreamConfig);
            var tlist = new List<string>
                    {
                        CreateTparam(ReportRedefinition.Report_RPropertyName, reportfilename, type),
                        CreateTparam(ReportRedefinition.Host_RPropertyName, host, type),
                        CreateTparam(ReportRedefinition.PartnerId_RPropertyName, mandantcode, type)
                    };

            if (entity != null)
            {
                var entityprdsc = TypeDescriptor.GetProperties(entity.GetType());
                var typeofReportRedefinition = typeof(ReportRedefinition);
                var reportRedefinitionprdsc = TypeDescriptor.GetProperties(typeofReportRedefinition);
                foreach (PropertyDescriptor p in reportRedefinitionprdsc)
                {
                    var property = entityprdsc.Find(p.Name, true);
                    if (property != null &&
                        !property.Name.EqIgnoreCase(WMSBusinessObject.UserInsPropertyName) && !property.Name.EqIgnoreCase(WMSBusinessObject.DateInsPropertyName) &&
                        !property.Name.EqIgnoreCase(WMSBusinessObject.UserUpdPropertyName) && !property.Name.EqIgnoreCase(WMSBusinessObject.DateUpdPropertyName))
                    {
                        tlist.Add(CreateTparam(p.Name, property.GetValue(entity), typeofReportRedefinition));
                    }
                }
            }

            return GetDefaultReport(CreateTListParams(tlist));
        }

        public ReportRedefinition GetDefaultReport(string tListParams)
        {
            using (var repo = GetRepository<IReport2EntityRepository>())
                return repo.GetDefaultReport(tListParams);
        }

        private string CreateTparam(string propertyname, object value, Type type)
        {
            const string tparam = "TParam";
            return string.Format(tparam + "('{0}',{1},null)",
                type == null ? propertyname : SourceNameHelper.Instance.GetPropertySourceName(type, propertyname).ToUpper(),
                value is string ? string.Format("'{0}'", value) : value ?? "null");
        }

        private string CreateTListParams(IEnumerable<string> tlist)
        {
            return string.Format("TListParams({0})", string.Join(",", tlist));
        }
    }

    public class PrintReportStatus
    {
        public PrintReportStatus() { }
        public PrintReportStatus(string message)
        {
            Error = message;
        }
        public PrintReportStatus(string messageFormat, params object[] args)
        {
            Error = string.Format(messageFormat, args);
        }

        /// <summary>
        /// Key EPS задания на печать.
        /// </summary>
        public string Job { get; set; }

        /// <summary>
        /// Наименование физического принтера.
        /// </summary>
        public string Printer { get; set; }

        public string Error { get; set; }

        public bool HasError { get { return !string.IsNullOrEmpty(Error); } }
    }
}