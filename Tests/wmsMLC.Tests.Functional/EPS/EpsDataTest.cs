using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using wmsMLC.Business;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Tests.Functional.EPS
{
    [TestFixture, Ignore("Отдельное тестирование")]
    public class EpsDataTest
    {
        private const string Login = "DEBUG";
        private const int EpsHandler = 1;
        private const string Email = "my@my.ru";

        [TestFixtureSetUp]
        public void Setup()
        {
            BLHelper.InitBL(dalType: DALType.Oracle);
            var auth = IoC.Instance.Resolve<IAuthenticationProvider>();
            auth.Authenticate(Login, "DEBUG");
        }

        [Test]
        public Output CreateTask()
        {
            //Параметры отчета
            var epsReports = new Dictionary<string, Variable[]>
                {
                    {
                        "Test001.frx", new[]
                            {
                                new Variable {Value = "Parameter1", SubValue = "test0011"},
                                new Variable {Value = "Parameter2", SubValue = "test0012"},
                            }
                    },
                    {
                        "Test002.frx", new[]
                            {
                                new Variable {Value = "Parameter1", SubValue = "test0021"},
                                new Variable {Value = "Parameter2", SubValue = "test0022"},
                            }
                    },

                };

            var resultReportFile = "Myoutput_${REPORTNAME}";

            return CreateTask(login_r: "EpsTest", epsHandler: EpsHandler, epsReports: epsReports,
                resultReportFile: resultReportFile, changeOdbc: null, useZip: true, useReserveCopy: true,
                outputTaskparams: new[] { CreateOutputTasks_OTC_MAIL(1),  });
                //outputTaskparams: new[] { CreateOutputTasks_OTC_MAIL(1), CreateOutputTasks_OTC_FTP(2, false, EpsTaskProtect.CRC) });
        }

        /// <summary>
        /// Передать клиенту.
        /// </summary>
        /// <param name="outputTaskOrder">Порядок выполнения задачи</param>
        /// <returns></returns>
        public OutputTask CreateOutputTasks_OTC_DCL(int outputTaskOrder)
        {
            var taskOutputParam = new List<TaskOutputParam>();
            foreach (
                var epsTaskParam in
                    Enum.GetValues(typeof(EpsTaskParams))
                        .Cast<EpsTaskParams>()
                        .Where(epsTaskParam => epsTaskParam != EpsTaskParams.None
                                               && epsTaskParam != EpsTaskParams.Variable &&
                                               epsTaskParam != EpsTaskParams.EpsReport &&
                                               epsTaskParam != EpsTaskParams.ResultReportFile &&
                                               epsTaskParam != EpsTaskParams.ChangeODBC
                                               && epsTaskParam != EpsTaskParams.Zip &&
                                               epsTaskParam != EpsTaskParams.ReserveCopy))
            {
                var item = new TaskOutputParam { Code = epsTaskParam };
                switch (epsTaskParam)
                {
                    case EpsTaskParams.AsAttachment:
                        break;
                    case EpsTaskParams.Copies:
                        break;
                    case EpsTaskParams.CopyFile:
                        break;
                    case EpsTaskParams.Email:
                        item.Value = Email;
                        break;
                    case EpsTaskParams.FTPEncodingFile:
                        break;
                    case EpsTaskParams.FTPFolder:
                        break;
                    case EpsTaskParams.FTPServerLogin:
                        break;
                    case EpsTaskParams.FTPServerName:
                        break;
                    case EpsTaskParams.FTPServerPassword:
                        break;
                    case EpsTaskParams.FTPTransmissionMode:
                        break;
                    case EpsTaskParams.FileFormat:
                        item.Value = GetFileFormat(3);
                        break;
                    case EpsTaskParams.FileMask:
                        break;
                    case EpsTaskParams.FileRecordProtect:
                        break;
                    case EpsTaskParams.MoveFile:
                        break;
                    case EpsTaskParams.PhysicalPrinter:
                        break;
                    case EpsTaskParams.SendBlankMail:
                        break;
                    case EpsTaskParams.SourceFolder:
                        break;
                    case EpsTaskParams.SupportFileName:
                        break;
                    case EpsTaskParams.SupportTargetFolder:
                        break;
                    case EpsTaskParams.TargetFolder:
                        break;
                    case EpsTaskParams.FileExtension:
                        break;
                    case EpsTaskParams.WorkflowIdentify:
                        break;
                    default:
                        throw new DeveloperException(string.Format("Данный тип параметра '{0}' не обрабатывается.", epsTaskParam));
                }
                taskOutputParam.Add(item);
            }
            return CreateOutputTasks(task: EpsTaskType.OTC_DCL, outputTaskOrder: outputTaskOrder, taskOutputParam: taskOutputParam.Any() ? taskOutputParam.ToArray() : null);
        }

        /// <summary>
        /// Передать по FTP.
        /// </summary>
        /// <param name="outputTaskOrder">Порядок выполнения задачи</param>
        /// <param name="useMoveFile">Если useMoveFile == true, то вместо CopyFile используем MoveFile</param>
        /// <param name="epsTaskProtect">Значение парамета FileRecordProtect</param>
        public OutputTask CreateOutputTasks_OTC_FTP(int outputTaskOrder, bool useMoveFile, EpsTaskProtect epsTaskProtect)
        {
            var taskOutputParam = new List<TaskOutputParam>();
            foreach (
                var epsTaskParam in
                    Enum.GetValues(typeof(EpsTaskParams))
                        .Cast<EpsTaskParams>()
                        .Where(epsTaskParam => epsTaskParam != EpsTaskParams.None
                                               && epsTaskParam != EpsTaskParams.Variable &&
                                               epsTaskParam != EpsTaskParams.EpsReport &&
                                               epsTaskParam != EpsTaskParams.ResultReportFile &&
                                               epsTaskParam != EpsTaskParams.ChangeODBC
                                               && epsTaskParam != EpsTaskParams.Zip &&
                                               epsTaskParam != EpsTaskParams.ReserveCopy

                                               //исключаеи неиспользуемые параметры
                                               && epsTaskParam != EpsTaskParams.AsAttachment
                                               && epsTaskParam != EpsTaskParams.Copies
                                               && epsTaskParam != EpsTaskParams.Email
                                               && epsTaskParam != EpsTaskParams.PhysicalPrinter
                                               && epsTaskParam != EpsTaskParams.SendBlankMail
                                               && epsTaskParam != EpsTaskParams.WorkflowIdentify

                                               //исключаем альтернативные параметры
                                               && epsTaskParam != EpsTaskParams.FTPFolder
                                               && epsTaskParam != EpsTaskParams.FTPTransmissionMode
                                               ))
            {
                if (useMoveFile && epsTaskParam == EpsTaskParams.CopyFile) continue;

                var item = new TaskOutputParam { Code = epsTaskParam };
                //Если необходимы значения заполняем
                switch (epsTaskParam)
                {
                    case EpsTaskParams.AsAttachment:
                        break;
                    case EpsTaskParams.Copies:
                        break;
                    case EpsTaskParams.CopyFile:
                        break;
                    case EpsTaskParams.Email:
                        break;
                    case EpsTaskParams.FTPEncodingFile:
                        item.Value = "UTF8";
                        break;
                    case EpsTaskParams.FTPFolder:
                        break;
                    case EpsTaskParams.FTPServerLogin:
                        item.Value = "wmsmlc_ftp_local";
                        break;
                    case EpsTaskParams.FTPServerName:
                        item.Value = "10.0.2.223";
                        break;
                    case EpsTaskParams.FTPServerPassword:
                        item.Value = "Oy1ohpahJohH";
                        break;
                    case EpsTaskParams.FTPTransmissionMode:
                        break;
                    case EpsTaskParams.FileFormat:
                        item.Value = GetFileFormat(3);
                        break;
                    case EpsTaskParams.FileMask:
                        item.Value = "*";
                        break;
                    case EpsTaskParams.FileRecordProtect:
                        item.Value = epsTaskProtect.ToString();
                        break;
                    case EpsTaskParams.MoveFile:
                        break;
                    case EpsTaskParams.PhysicalPrinter:
                        break;
                    case EpsTaskParams.SendBlankMail:
                        break;
                    case EpsTaskParams.SourceFolder:
                        item.Value = @"\\mp-app-t1-nwms\wmsMLCTest\EPS\msg\SIT 1";
                        break;
                    case EpsTaskParams.SupportFileName:
                        item.Value = "2791_msg_test_A4-1.frx";
                        break;
                    case EpsTaskParams.SupportTargetFolder:
                        item.Value = @"TEST\INBOUND\TestSTF";
                        break;
                    case EpsTaskParams.TargetFolder:
                        item.Value = @"TEST\INBOUND\TestTF";
                        break;
                    case EpsTaskParams.FileExtension:
                        item.Value = "myext";
                        break;
                    case EpsTaskParams.WorkflowIdentify:
                        break;

                    default:
                        throw new DeveloperException(string.Format("Данный тип параметра '{0}' не обрабатывается.", epsTaskParam));
                }
                taskOutputParam.Add(item);
            }
            return CreateOutputTasks(task: EpsTaskType.OTC_FTP, outputTaskOrder: outputTaskOrder,
                                     taskOutputParam: taskOutputParam.Any() ? taskOutputParam.ToArray() : null);
        }

        public OutputTask CreateOutputTasks_OTC_MAIL(int outputTaskOrder)
        {
            var taskOutputParam = new List<TaskOutputParam>();
            foreach (
                var epsTaskParam in
                    Enum.GetValues(typeof(EpsTaskParams))
                        .Cast<EpsTaskParams>()
                        .Where(epsTaskParam => epsTaskParam != EpsTaskParams.None
                                               && epsTaskParam != EpsTaskParams.Variable &&
                                               epsTaskParam != EpsTaskParams.EpsReport &&
                                               epsTaskParam != EpsTaskParams.ResultReportFile &&
                                               epsTaskParam != EpsTaskParams.ChangeODBC
                                               && epsTaskParam != EpsTaskParams.Zip &&
                                               epsTaskParam != EpsTaskParams.ReserveCopy
                            
                                               //исключаеи неиспользуемые параметры
                                               && epsTaskParam != EpsTaskParams.AsAttachment
                                               && epsTaskParam != EpsTaskParams.Copies
                                               && epsTaskParam != EpsTaskParams.CopyFile
                                               && epsTaskParam != EpsTaskParams.SendBlankMail
                                               && epsTaskParam != EpsTaskParams.FTPEncodingFile
                                               && epsTaskParam != EpsTaskParams.FTPFolder
                                               && epsTaskParam != EpsTaskParams.FTPServerName
                                               && epsTaskParam != EpsTaskParams.FTPServerPassword
                                               && epsTaskParam != EpsTaskParams.FTPTransmissionMode
                                               && epsTaskParam != EpsTaskParams.FileMask
                                               && epsTaskParam != EpsTaskParams.FileRecordProtect
                                               && epsTaskParam != EpsTaskParams.MoveFile
                                               && epsTaskParam != EpsTaskParams.PhysicalPrinter
                                               && epsTaskParam != EpsTaskParams.SourceFolder
                                               && epsTaskParam != EpsTaskParams.SupportFileName
                                               && epsTaskParam != EpsTaskParams.WorkflowIdentify

                                               && epsTaskParam != EpsTaskParams.SupportTargetFolder
                                               && epsTaskParam != EpsTaskParams.TargetFolder
                                               && epsTaskParam != EpsTaskParams.FTPServerLogin
                                               ))
            {

                var item = new TaskOutputParam { Code = epsTaskParam };
                switch (epsTaskParam)
                {
                    case EpsTaskParams.AsAttachment:
                        break;
                    case EpsTaskParams.Copies:
                        break;
                    case EpsTaskParams.CopyFile:
                        break;
                    case EpsTaskParams.Email:
                        item.Value = Email;
                        break;
                    case EpsTaskParams.FTPEncodingFile:
                        break;
                    case EpsTaskParams.FTPFolder:
                        break;
                    case EpsTaskParams.FTPServerLogin:
                        break;
                    case EpsTaskParams.FTPServerName:
                        break;
                    case EpsTaskParams.FTPServerPassword:
                        break;
                    case EpsTaskParams.FTPTransmissionMode:
                        break;
                    case EpsTaskParams.FileFormat:
                        item.Value = GetFileFormat(5);
                        break;
                    case EpsTaskParams.FileMask:
                        break;
                    case EpsTaskParams.FileRecordProtect:
                        break;
                    case EpsTaskParams.MoveFile:
                        break;
                    case EpsTaskParams.PhysicalPrinter:
                        break;
                    case EpsTaskParams.SendBlankMail:
                        break;
                    case EpsTaskParams.SourceFolder:
                        break;
                    case EpsTaskParams.SupportFileName:
                        break;
                    case EpsTaskParams.SupportTargetFolder:
                        break;
                    case EpsTaskParams.TargetFolder:
                        break;
                    case EpsTaskParams.WorkflowIdentify:
                        break;

                    default:
                        break;
                }
                taskOutputParam.Add(item);
            }
            return CreateOutputTasks(task: EpsTaskType.OTC_MAIL, outputTaskOrder: outputTaskOrder,
                                     taskOutputParam: taskOutputParam.Any() ? taskOutputParam.ToArray() : null);
        }

        public OutputTask CreateOutputTasks_OTC_PRINT(int outputTaskOrder)
        {
            var taskOutputParam = new List<TaskOutputParam>();
            foreach (
                var epsTaskParam in
                    Enum.GetValues(typeof(EpsTaskParams))
                        .Cast<EpsTaskParams>()
                        .Where(epsTaskParam => epsTaskParam != EpsTaskParams.None
                                               && epsTaskParam != EpsTaskParams.Variable &&
                                               epsTaskParam != EpsTaskParams.EpsReport &&
                                               epsTaskParam != EpsTaskParams.ResultReportFile &&
                                               epsTaskParam != EpsTaskParams.ChangeODBC
                                               && epsTaskParam != EpsTaskParams.Zip &&
                                               epsTaskParam != EpsTaskParams.ReserveCopy))
            {

                var item = new TaskOutputParam { Code = epsTaskParam };
                switch (epsTaskParam)
                {
                    case EpsTaskParams.AsAttachment:
                        break;
                    case EpsTaskParams.Copies:
                        break;
                    case EpsTaskParams.CopyFile:
                        break;
                    case EpsTaskParams.Email:
                        item.Value = Email;
                        break;
                    case EpsTaskParams.FTPEncodingFile:
                        break;
                    case EpsTaskParams.FTPFolder:
                        break;
                    case EpsTaskParams.FTPServerLogin:
                        break;
                    case EpsTaskParams.FTPServerName:
                        break;
                    case EpsTaskParams.FTPServerPassword:
                        break;
                    case EpsTaskParams.FTPTransmissionMode:
                        break;
                    case EpsTaskParams.FileFormat:
                        item.Value = GetFileFormat(6);
                        break;
                    case EpsTaskParams.FileMask:
                        break;
                    case EpsTaskParams.FileRecordProtect:
                        break;
                    case EpsTaskParams.MoveFile:
                        break;
                    case EpsTaskParams.PhysicalPrinter:
                        break;
                    case EpsTaskParams.SendBlankMail:
                        break;
                    case EpsTaskParams.SourceFolder:
                        break;
                    case EpsTaskParams.SupportFileName:
                        break;
                    case EpsTaskParams.SupportTargetFolder:
                        break;
                    case EpsTaskParams.TargetFolder:
                        break;
                    case EpsTaskParams.WorkflowIdentify:
                        break;

                    default:
                        break;
                }
                taskOutputParam.Add(item);
            }
            return CreateOutputTasks(task: EpsTaskType.OTC_PRINT, outputTaskOrder: outputTaskOrder,
                                     taskOutputParam: taskOutputParam.Any() ? taskOutputParam.ToArray() : null);
        }

        public OutputTask CreateOutputTasks_OTC_SHARE(int outputTaskOrder)
        {
            var taskOutputParam = new List<TaskOutputParam>();
            foreach (
                var epsTaskParam in
                    Enum.GetValues(typeof(EpsTaskParams))
                        .Cast<EpsTaskParams>()
                        .Where(epsTaskParam => epsTaskParam != EpsTaskParams.None
                                               && epsTaskParam != EpsTaskParams.Variable &&
                                               epsTaskParam != EpsTaskParams.EpsReport &&
                                               epsTaskParam != EpsTaskParams.ResultReportFile &&
                                               epsTaskParam != EpsTaskParams.ChangeODBC
                                               && epsTaskParam != EpsTaskParams.Zip &&
                                               epsTaskParam != EpsTaskParams.ReserveCopy))
            {

                var item = new TaskOutputParam { Code = epsTaskParam };
                switch (epsTaskParam)
                {
                    case EpsTaskParams.AsAttachment:
                        break;
                    case EpsTaskParams.Copies:
                        break;
                    case EpsTaskParams.CopyFile:
                        break;
                    case EpsTaskParams.Email:
                        item.Value = Email;
                        break;
                    case EpsTaskParams.FTPEncodingFile:
                        break;
                    case EpsTaskParams.FTPFolder:
                        break;
                    case EpsTaskParams.FTPServerLogin:
                        break;
                    case EpsTaskParams.FTPServerName:
                        break;
                    case EpsTaskParams.FTPServerPassword:
                        break;
                    case EpsTaskParams.FTPTransmissionMode:
                        break;
                    case EpsTaskParams.FileFormat:
                        item.Value = GetFileFormat(7);
                        break;
                    case EpsTaskParams.FileMask:
                        break;
                    case EpsTaskParams.FileRecordProtect:
                        break;
                    case EpsTaskParams.MoveFile:
                        break;
                    case EpsTaskParams.PhysicalPrinter:
                        break;
                    case EpsTaskParams.SendBlankMail:
                        break;
                    case EpsTaskParams.SourceFolder:
                        break;
                    case EpsTaskParams.SupportFileName:
                        break;
                    case EpsTaskParams.SupportTargetFolder:
                        break;
                    case EpsTaskParams.TargetFolder:
                        break;
                    case EpsTaskParams.WorkflowIdentify:
                        break;

                    default:
                        break;
                }
                taskOutputParam.Add(item);
            }
            return CreateOutputTasks(task: EpsTaskType.OTC_SHARE, outputTaskOrder: outputTaskOrder,
                                     taskOutputParam: taskOutputParam.Any() ? taskOutputParam.ToArray() : null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputTaskOrder">Порядок выполнения задачи</param>
        /// <returns></returns>
        public OutputTask CreateOutputTasks_OTC_ARCH(int outputTaskOrder)
        {
            var taskOutputParam = new List<TaskOutputParam>();
            foreach (
                var epsTaskParam in
                    Enum.GetValues(typeof(EpsTaskParams))
                        .Cast<EpsTaskParams>()
                        .Where(epsTaskParam => epsTaskParam != EpsTaskParams.None
                                               && epsTaskParam != EpsTaskParams.Variable &&
                                               epsTaskParam != EpsTaskParams.EpsReport &&
                                               epsTaskParam != EpsTaskParams.ResultReportFile &&
                                               epsTaskParam != EpsTaskParams.ChangeODBC
                                               && epsTaskParam != EpsTaskParams.Zip &&
                                               epsTaskParam != EpsTaskParams.ReserveCopy))
            {

                var item = new TaskOutputParam { Code = epsTaskParam };
                switch (epsTaskParam)
                {
                    case EpsTaskParams.AsAttachment:
                        break;
                    case EpsTaskParams.Copies:
                        break;
                    case EpsTaskParams.CopyFile:
                        break;
                    case EpsTaskParams.Email:
                        item.Value = Email;
                        break;
                    case EpsTaskParams.FTPEncodingFile:
                        break;
                    case EpsTaskParams.FTPFolder:
                        break;
                    case EpsTaskParams.FTPServerLogin:
                        break;
                    case EpsTaskParams.FTPServerName:
                        break;
                    case EpsTaskParams.FTPServerPassword:
                        break;
                    case EpsTaskParams.FTPTransmissionMode:
                        break;
                    case EpsTaskParams.FileFormat:
                        item.Value = GetFileFormat(3);
                        break;
                    case EpsTaskParams.FileMask:
                        break;
                    case EpsTaskParams.FileRecordProtect:
                        break;
                    case EpsTaskParams.MoveFile:
                        break;
                    case EpsTaskParams.PhysicalPrinter:
                        break;
                    case EpsTaskParams.SendBlankMail:
                        break;
                    case EpsTaskParams.SourceFolder:
                        break;
                    case EpsTaskParams.SupportFileName:
                        break;
                    case EpsTaskParams.SupportTargetFolder:
                        break;
                    case EpsTaskParams.TargetFolder:
                        break;
                    case EpsTaskParams.WorkflowIdentify:
                        break;

                    default:
                        break;
                }
                taskOutputParam.Add(item);
            }
            return CreateOutputTasks(task: EpsTaskType.OTC_ARCH, outputTaskOrder: outputTaskOrder, taskOutputParam: taskOutputParam.Any() ? taskOutputParam.ToArray() : null);
        }

        public Output CreateTask(string login_r, int epsHandler,
                               Dictionary<string, Variable[]> epsReports, string resultReportFile, bool? changeOdbc,
                               bool useZip, bool useReserveCopy,
                               params OutputTask[] outputTaskparams)
        {
            var manager = CreateManager<Output>();
            var output = manager.New();
            output.Login_R = login_r;
            output.Host_R = Environment.MachineName;
            output.OutputStatus = OutputStatus.OS_NEW.ToString();
            output.EpsHandler = EpsHandler;
            output.ReportFileSubfolder_R = null;

            //Параметры отчета
            output.ReportParams = CreateReportParams(epsReports, resultReportFile, changeOdbc);
            output.EpsParams = CreateEpsParams(useZip, useReserveCopy);
            if (outputTaskparams != null)
            {
                if (output.OutputTasks == null) output.OutputTasks = new WMSBusinessCollection<OutputTask>();
                foreach (var p in outputTaskparams.Where(p => p != null))
                {
                    output.OutputTasks.Add(p);
                }

                if (!output.OutputTasks.Any()) output.OutputTasks = null;
            }
            manager.Insert(ref output);
            var key = ((IKeyHandler)output).GetKey();
            Trace.WriteLine(string.Format("Сформирована задача ESP с кодом {0}.", key));
            return output;
        }

        public WMSBusinessCollection<OutputParam> CreateReportParams(Dictionary<string, Variable[]> epsReports, string resultReportFile, bool? changeOdbc)
        {
            var result = new WMSBusinessCollection<OutputParam>();
            if (epsReports != null)
            {
                foreach (var p in epsReports)
                {
                    if (!string.IsNullOrEmpty(p.Key))
                    {
                        result.Add(new OutputParam
                        {
                            OutputParamCode = EpsTaskParams.EpsReport.ToString(),
                            OutputParamValue = p.Key,
                            OutputParamType = EpsParamType.REP.ToString()
                        });
                        if (p.Value != null)
                        {
                            foreach (var v in p.Value.Where(v => !string.IsNullOrEmpty(v.Value)))
                            {
                                result.Add(new OutputParam
                                {
                                    OutputParamCode = v.Value,
                                    OutputParamValue = v.SubValue,
                                    OutputParamSubvalue = p.Key,
                                    OutputParamType = EpsParamType.REP.ToString()
                                });
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(resultReportFile))
            {
                result.Add(new OutputParam
                {
                    OutputParamCode = EpsTaskParams.ResultReportFile.ToString(),
                    OutputParamValue = resultReportFile,
                    OutputParamType = EpsParamType.REP.ToString()
                });
            }

            if (changeOdbc.HasValue)
            {
                result.Add(new OutputParam
                {
                    OutputParamCode = EpsTaskParams.ChangeODBC.ToString(),
                    OutputParamValue = (changeOdbc.Value ? 1 : 0).ToString(CultureInfo.InvariantCulture),
                    OutputParamType = EpsParamType.REP.ToString()
                });
            }

            return result.Any() ? result : null;
        }

        public WMSBusinessCollection<OutputParam> CreateEpsParams(bool useZip, bool useReserveCopy)
        {
            var result = new WMSBusinessCollection<OutputParam>();
            if (useZip)
            {
                result.Add(new OutputParam
                {
                    OutputParamCode = EpsTaskParams.Zip.ToString(),
                    OutputParamType = EpsParamType.EPS.ToString()
                });
            }
            if (useReserveCopy)
            {
                result.Add(new OutputParam
                {
                    OutputParamCode = EpsTaskParams.ReserveCopy.ToString(),
                    OutputParamType = EpsParamType.EPS.ToString()
                });
            }
            return result.Any() ? result : null;
        }

        public OutputTask CreateOutputTasks(EpsTaskType task, int outputTaskOrder, TaskOutputParam[] taskOutputParam)
        {
            if (task == EpsTaskType.None) return null;
            var result = new OutputTask
            {
                OutputTaskCode = task.ToString(),
                OutputTaskOrder = outputTaskOrder,
            };

            if (taskOutputParam != null && taskOutputParam.Length > 0)
            {
                result.TaskParams = new WMSBusinessCollection<OutputParam>();
                foreach (var p in taskOutputParam.Where(p => p != null && p.Code != EpsTaskParams.None
                                                             && p.Code != EpsTaskParams.Variable &&
                                                             p.Code != EpsTaskParams.EpsReport &&
                                                             p.Code != EpsTaskParams.ResultReportFile &&
                                                             p.Code != EpsTaskParams.ChangeODBC
                                                             && p.Code != EpsTaskParams.Zip &&
                                                             p.Code != EpsTaskParams.ReserveCopy))
                {
                    result.TaskParams.Add(new OutputParam
                    {
                        OutputParamCode = p.Code.ToString(),
                        OutputParamValue = p.Value,
                        OutputParamType = EpsParamType.TSK.ToString()
                    });
                }
            }

            return result;
        }

        public string GetFileFormat(int formatid)
        {
            switch (formatid)
            {
                case 0:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FRBmp";
                case 1:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FRCsv";
                case 2:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FRDbf";
                case 3:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FRExcel2007";
                case 4:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FRGif";
                case 5:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FRHtml";
                case 6:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FRJpeg";
                case 7:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FRMetafile";
                case 8:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FRMht";
                case 9:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FRMhtml";
                case 10:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FROdf";
                case 11:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FROds";
                case 12:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FROdt";
                case 13:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FROoBase";
                case 14:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FRPdf";
                case 15:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FRPowerPoint2007";
                case 16:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FRRtf";
                case 17:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FRText";
                case 18:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FRTiff";
                case 19:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FRPng";
                case 20:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FRWord2007";
                case 21:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FRXml";
                case 22:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FRXps";
            }

            return "wmsMLC.EPS.wmsEPS.ExportTypes.FRPdf";
        }

        private IBaseManager<T> CreateManager<T>()
        {
            return IoC.Instance.Resolve<IBaseManager<T>>();
        }
    }
}
