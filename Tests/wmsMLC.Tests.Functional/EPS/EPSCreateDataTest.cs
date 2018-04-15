using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using FluentAssertions;
using wmsMLC.Business;
using wmsMLC.Business.Objects;
using wmsMLC.EPS.wmsEPS;
using wmsMLC.EPS.wmsEPS.Helpers;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.Services.Service;
namespace wmsMLC.Tests.Functional.EPS
{
    [TestFixture, Ignore("Отдельное тестирование")]
    //[TestFixture]
    public class EpsTest
    {
        private const string Login = "DEBUG";
        private const int EpsHandler = 1;
        private const string Email = "wms_server@my.ru";

        private const string FtpServerName = "10.0.2.223";
        private const string FtpEncodingFile = "UTF8";
        private const string FtpServerLogin = "wmsmlc_ftp_local";
        private const string FtpServerPassword = "Oy1ohpahJohH";
        private const string FtpSupportTargetFolder = @"TEST\INBOUND\TestSTF";
        private const string FtpSupportTargetFolderCon = @"TEST\INBOUND\TestSTF\con";
        private const string FtpTargetFolder = @"TEST\INBOUND\TestTF";
        private const string FtpTargetFolderCon = @"TEST\INBOUND\TestTF\con";
        private const string SourceFolder = @"\\mp-app-t1-nwms\wmsMLCTest\EPS\msg\source";
        private const string SourceFolderCon = @"\\mp-app-t1-nwms\wmsMLCTest\EPS\msg\source\con";
        private const string TargetFolder = @"\\mp-app-t1-nwms\wmsMLCTest\EPS\msg\Target";
        private const string TargetFolderCon = @"\\mp-app-t1-nwms\wmsMLCTest\EPS\msg\Target\con";
        private const string SupportTargetFolder = @"\\mp-app-t1-nwms\wmsMLCTest\EPS\msg\Support";
        private const string SupportTargetFolderCon = @"\\mp-app-t1-nwms\wmsMLCTest\EPS\msg\Support\con";
        private const string PhysicalPrinter = "eDocPrintPro";

        private List<EpsTaskTypeStruct> _validParams = new List<EpsTaskTypeStruct>();
        private List<EpsTaskTypeStruct> _invalidParams = new List<EpsTaskTypeStruct>();
        private List<EpsTaskTypeStruct> _parallelParams = new List<EpsTaskTypeStruct>();

        //private ConfigManagerBase _configManager;
        private ConfigBase _config;

        #region . CreateParams .

        // Набор параметров для задачи OTC_DCL
        public TaskOutputParam[] CreateDclParams(string fileFormat, string[] fileMask, bool move, bool valid)
        {
            var pars = new List<TaskOutputParam>();
            foreach (var mask in fileMask)
            {
                // Eps Маска файла (FileMask)
                pars.Add(new TaskOutputParam { Code = EpsTaskParams.FileMask, Value = (valid) ? mask : string.Empty });
            }
            // Eps Переместить файл (MoveFile)
            if (move)
                pars.Add(new TaskOutputParam { Code = EpsTaskParams.MoveFile, Value = null });
            // Eps Сетевой каталог-источник файла (SourceFolder)
            pars.Add(new TaskOutputParam { Code = EpsTaskParams.SourceFolder, Value = (valid) ? SourceFolder : SourceFolderCon });
            // Eps Сетевой каталог-цель файла (TargetFolder)
            pars.Add(new TaskOutputParam { Code = EpsTaskParams.TargetFolder, Value = (valid) ? TargetFolder : TargetFolderCon });
            // Eps Скопировать файл (CopyFile)
            if (!move)
                pars.Add(new TaskOutputParam { Code = EpsTaskParams.CopyFile, Value = null });
            // Eps Формат файла (FileFormat)
            pars.Add(new TaskOutputParam { Code = EpsTaskParams.FileFormat, Value = (valid) ? fileFormat : string.Empty });
            return pars.ToArray();
        }

        // Набор параметров для задачи OTC_SHARE
        public TaskOutputParam[] CreateShareParams(string fileFormat, EpsTaskProtect protect, string[] fileMask, bool move, bool valid)
        {
            var pars = new List<TaskOutputParam>();
            // Eps Вспомогательный каталог (SupportTargetFolder)
            pars.Add(new TaskOutputParam { Code = EpsTaskParams.SupportTargetFolder, Value = (valid) ? SupportTargetFolder : SupportTargetFolderCon });
            // Eps Защита файла во время записи (FileRecordProtect)
            pars.Add(new TaskOutputParam { Code = EpsTaskParams.FileRecordProtect, Value = (valid) ? protect.ToString() : string.Empty });
            // Eps Имя вспомогательного файла (SupportFileName)
            pars.Add(new TaskOutputParam { Code = EpsTaskParams.SupportFileName, Value = (valid) ? Guid.NewGuid().ToString() : string.Empty });
            foreach (var mask in fileMask)
            {
                // Eps Маска файла (FileMask)
                pars.Add(new TaskOutputParam { Code = EpsTaskParams.FileMask, Value = (valid) ? mask : string.Empty });
            }
            // Eps Переместить файл (MoveFile)
            if (move)
                pars.Add(new TaskOutputParam { Code = EpsTaskParams.MoveFile, Value = null });
            // Eps Сетевой каталог-источник файла (SourceFolder)
            pars.Add(new TaskOutputParam { Code = EpsTaskParams.SourceFolder, Value = (valid) ? SourceFolder : SourceFolderCon });
            // Eps Сетевой каталог-цель файла (TargetFolder)
            pars.Add(new TaskOutputParam { Code = EpsTaskParams.TargetFolder, Value = (valid) ? TargetFolder : TargetFolderCon });
            // Eps Скопировать файл (CopyFile)
            if (!move)
                pars.Add(new TaskOutputParam { Code = EpsTaskParams.CopyFile, Value = null });
            // Eps Формат файла (FileFormat)
            pars.Add(new TaskOutputParam { Code = EpsTaskParams.FileFormat, Value = (valid) ? fileFormat : "" });
            return pars.ToArray();
        }

        // Набор параметров для задачи OTC_FTP
        public TaskOutputParam[] CreateFtpParams(string fileFormat, int mode, EpsTaskProtect protect, string[] fileMask, bool move, bool valid)
        {
            var pars = new List<TaskOutputParam>();
            // Eps Имя FTP-сервера (FTPServerName)
            pars.Add(new TaskOutputParam { Code = EpsTaskParams.FTPServerName, Value = (valid) ? FtpServerName : string.Empty });
            // Eps Кодировка имен файлов FTP (FTPEncodingFile)
            pars.Add(new TaskOutputParam { Code = EpsTaskParams.FTPEncodingFile, Value = (valid) ? FtpEncodingFile : string.Empty });
            // Eps Логин пользователя FTP (FTPServerLogin)
            pars.Add(new TaskOutputParam { Code = EpsTaskParams.FTPServerLogin, Value = (valid) ? FtpServerLogin : string.Empty });
            // Eps Пароль пользователя FTP (FTPServerPassword)
            pars.Add(new TaskOutputParam { Code = EpsTaskParams.FTPServerPassword, Value = (valid) ? FtpServerPassword : string.Empty });
            // Eps Режим обмена с FTP (FTPTransmissionMode)
            pars.Add(new TaskOutputParam { Code = EpsTaskParams.FTPTransmissionMode, Value = (valid) ? mode.ToString() : string.Empty });

            // Eps Вспомогательный каталог (SupportTargetFolder)
            pars.Add(new TaskOutputParam { Code = EpsTaskParams.SupportTargetFolder, Value = (valid) ? FtpSupportTargetFolder : FtpSupportTargetFolderCon });
            // Eps Защита файла во время записи (FileRecordProtect)
            pars.Add(new TaskOutputParam { Code = EpsTaskParams.FileRecordProtect, Value = (valid) ? protect.ToString() : string.Empty });
            // Eps Имя вспомогательного файла (SupportFileName)
            pars.Add(new TaskOutputParam { Code = EpsTaskParams.SupportFileName, Value = (valid) ? Guid.NewGuid().ToString() : string.Empty });
            foreach (var mask in fileMask)
            {
                // Eps Маска файла (FileMask)
                pars.Add(new TaskOutputParam { Code = EpsTaskParams.FileMask, Value = (valid) ? mask : string.Empty });
            }
            // Eps Переместить файл (MoveFile)
            if (move)
                pars.Add(new TaskOutputParam { Code = EpsTaskParams.MoveFile, Value = null });
            // Eps Сетевой каталог-источник файла (SourceFolder)
            pars.Add(new TaskOutputParam { Code = EpsTaskParams.SourceFolder, Value = (valid) ? SourceFolder : SourceFolderCon });
            // Eps Сетевой каталог-цель файла (TargetFolder)
            pars.Add(new TaskOutputParam { Code = EpsTaskParams.TargetFolder, Value = (valid) ? FtpTargetFolder : FtpTargetFolderCon });
            // Eps Скопировать файл (CopyFile)
            if (!move)
                pars.Add(new TaskOutputParam { Code = EpsTaskParams.CopyFile, Value = null });
            // Eps Формат файла (FileFormat)
            pars.Add(new TaskOutputParam { Code = EpsTaskParams.FileFormat, Value = (valid) ? fileFormat : "" });
            return pars.ToArray();
        }

        // Набор параметров для задачи OTC_MAIL
        public TaskOutputParam[] CreateMailParams(string fileFormat, string[] mails, string[] fileMask, int atach, bool move, bool valid)
        {
            var pars = new List<TaskOutputParam>();
            foreach (var mask in fileMask)
            {
                // Eps Маска файла (FileMask)
                pars.Add(new TaskOutputParam { Code = EpsTaskParams.FileMask, Value = (valid) ? mask : string.Empty });
            }
            // Eps Отправка пустого письма (SendBlankMail)
            pars.Add(new TaskOutputParam { Code = EpsTaskParams.SendBlankMail, Value = "1" });
            // Eps Переместить файл (MoveFile)
            if (move)
                pars.Add(new TaskOutputParam { Code = EpsTaskParams.MoveFile, Value = null });
            foreach (var mail in mails)
            {
                // Eps Почтовый адрес (Email)
                pars.Add(new TaskOutputParam { Code = EpsTaskParams.Email, Value = (valid) ? mail : string.Empty });
            }
            // Eps Сетевой каталог-источник файла (SourceFolder)
            pars.Add(new TaskOutputParam { Code = EpsTaskParams.SourceFolder, Value = (valid) ? SourceFolder : SourceFolderCon });
            // Eps Скопировать файл (CopyFile)
            if (!move)
                pars.Add(new TaskOutputParam { Code = EpsTaskParams.CopyFile, Value = null });
            // Eps Файл в виде вложения (AsAttachment)
            pars.Add(new TaskOutputParam { Code = EpsTaskParams.AsAttachment, Value = atach.ToString() });
            // Eps Формат файла (FileFormat)
            pars.Add(new TaskOutputParam { Code = EpsTaskParams.FileFormat, Value = (valid) ? fileFormat : "" });
            return pars.ToArray();
        }

        // Набор параметров для задачи OTC_PRINT
        public TaskOutputParam[] CreatePrintParams(string fileFormat, int copies, bool valid)
        {
            var pars = new List<TaskOutputParam>();
            // Eps Имя физического принтера (PhysicalPrinter)
            pars.Add(new TaskOutputParam { Code = EpsTaskParams.PhysicalPrinter, Value = (valid) ? PhysicalPrinter : string.Empty });
            // Eps Количество печатаемых копий (Copies)
            pars.Add(new TaskOutputParam { Code = EpsTaskParams.Copies, Value = (valid) ? copies.ToString() : "-1" });
            // Eps Формат файла (FileFormat)
            pars.Add(new TaskOutputParam { Code = EpsTaskParams.FileFormat, Value = (valid) ? fileFormat : "" });
            return pars.ToArray();
        }

        #endregion

        #region . Setup .
        [TestFixtureSetUp]
        public void Setup()
        {
            BLHelper.InitBL(dalType: DALType.Oracle);
            //BLHelper.RegisterServiceClient(Properties.Settings.Default.SessionId, Properties.Settings.Default.SDCL_Endpoint);
            //var auth = IoC.Instance.Resolve<IAuthenticationProvider>();
            //auth.Authenticate(Login, "DEBUG");
            //Инициализируем сервис. Считываем конфигурационные файлы
            //TODO: переделать на контекст
//            _configManager = ConfigManagerBase.Instance<EpsConfigManager>(null);
//            _configManager.SetDefaultValues();
//            _configManager.OnEventErrorHandler += (exception, type, code, message, process, id, name, userName) =>
//                                                 Trace.WriteLine(exception != null
//                                                                     ? string.Format(
//                                                                         "Ошибка в EpsConfigManager'е. {0}",
//                                                                         exception)
//                                                                     : string.Format(
//                                                                         "Сообщение EpsConfigManager'а. {0}",
//                                                                         process));
//
//
//            //TODO: переделать на контекст
//            //_configManager.ReadConfig<EpsConfig>(
//            //    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EPS"), "EPS.config");
//            _config = _configManager.GetConfig();
            // для каждого типа формата
            for (int i = 0; i < 22; i++)
            {
                // валидные параметры
                _validParams.Add(new EpsTaskTypeStruct { Type = EpsTaskType.OTC_DCL, Params = CreateDclParams(GetFileFormat(i), new string[] { "*", "*.frx" }, false, true) });
                _validParams.Add(new EpsTaskTypeStruct
                {
                    Type = EpsTaskType.OTC_SHARE,
                    Params = CreateShareParams(GetFileFormat(i), EpsTaskProtect.CRC, new string[] { "*" }, false, true)
                });
                _validParams.Add(new EpsTaskTypeStruct
                {
                    Type = EpsTaskType.OTC_SHARE,
                    Params = CreateShareParams(GetFileFormat(i), EpsTaskProtect.EXT, new string[] { "*" }, false, true)
                });
                _validParams.Add(new EpsTaskTypeStruct
                {
                    Type = EpsTaskType.OTC_SHARE,
                    Params = CreateShareParams(GetFileFormat(i), EpsTaskProtect.FOLDER, new string[] { "*" }, false, true)
                });
                _validParams.Add(new EpsTaskTypeStruct
                {
                    Type = EpsTaskType.OTC_FTP,
                    Params = CreateFtpParams(GetFileFormat(i), 0, EpsTaskProtect.CRC, new string[] { "*" }, false, true)
                });
                _validParams.Add(new EpsTaskTypeStruct
                {
                    Type = EpsTaskType.OTC_FTP,
                    Params = CreateFtpParams(GetFileFormat(i), 1, EpsTaskProtect.EXT, new string[] { "*" }, false, true)
                });
                _validParams.Add(new EpsTaskTypeStruct
                {
                    Type = EpsTaskType.OTC_FTP,
                    Params = CreateFtpParams(GetFileFormat(i), 1, EpsTaskProtect.FOLDER, new string[] { "*" }, false, true)
                });
                _validParams.Add(new EpsTaskTypeStruct
                {
                    Type = EpsTaskType.OTC_MAIL,
                    Params = CreateMailParams(GetFileFormat(i), new string[] { Email }, new string[] { "*" }, 1, false, true)
                });
                _validParams.Add(new EpsTaskTypeStruct
                {
                    Type = EpsTaskType.OTC_MAIL,
                    Params = CreateMailParams(GetFileFormat(i), new string[] { Email }, new string[] { "*" }, 0, false, true)
                });
                _validParams.Add(new EpsTaskTypeStruct
                {
                    Type = EpsTaskType.OTC_PRINT,
                    Params = CreatePrintParams(GetFileFormat(i), 0, true)
                });

                // не верные параметры
                _invalidParams.Add(new EpsTaskTypeStruct { Type = EpsTaskType.OTC_DCL, Params = CreateDclParams(GetFileFormat(i), new string[] { "*", "*.frx" }, false, false) });
                _invalidParams.Add(new EpsTaskTypeStruct
                {
                    Type = EpsTaskType.OTC_SHARE,
                    Params = CreateShareParams(GetFileFormat(i), EpsTaskProtect.CRC, new string[] { "*" }, false, false)
                });
                _invalidParams.Add(new EpsTaskTypeStruct
                {
                    Type = EpsTaskType.OTC_SHARE,
                    Params = CreateShareParams(GetFileFormat(i), EpsTaskProtect.EXT, new string[] { "*" }, false, false)
                });
                _invalidParams.Add(new EpsTaskTypeStruct
                {
                    Type = EpsTaskType.OTC_SHARE,
                    Params = CreateShareParams(GetFileFormat(i), EpsTaskProtect.FOLDER, new string[] { "*" }, false, false)
                });
                _invalidParams.Add(new EpsTaskTypeStruct
                {
                    Type = EpsTaskType.OTC_FTP,
                    Params = CreateFtpParams(GetFileFormat(i), 0, EpsTaskProtect.CRC, new string[] { "*" }, false, false)
                });
                _invalidParams.Add(new EpsTaskTypeStruct
                {
                    Type = EpsTaskType.OTC_FTP,
                    Params = CreateFtpParams(GetFileFormat(i), 1, EpsTaskProtect.EXT, new string[] { "*" }, false, false)
                });
                _invalidParams.Add(new EpsTaskTypeStruct
                {
                    Type = EpsTaskType.OTC_FTP,
                    Params = CreateFtpParams(GetFileFormat(i), 1, EpsTaskProtect.FOLDER, new string[] { "*" }, false, false)
                });
                _invalidParams.Add(new EpsTaskTypeStruct
                {
                    Type = EpsTaskType.OTC_MAIL,
                    Params = CreateMailParams(GetFileFormat(i), new string[] { Email }, new string[] { "*" }, 1, false, false)
                });
                _invalidParams.Add(new EpsTaskTypeStruct
                {
                    Type = EpsTaskType.OTC_MAIL,
                    Params = CreateMailParams(GetFileFormat(i), new string[] { Email }, new string[] { "*" }, 0, false, false)
                });
                _invalidParams.Add(new EpsTaskTypeStruct
                {
                    Type = EpsTaskType.OTC_PRINT,
                    Params = CreatePrintParams(GetFileFormat(i), 0, false)
                });
            }

            // валидные параметры с разными типами форматов
            _parallelParams.Add(new EpsTaskTypeStruct { Type = EpsTaskType.OTC_DCL, Params = CreateDclParams(GetFileFormat(0), new string[] { "*", "*.frx" }, false, true) });
            _parallelParams.Add(new EpsTaskTypeStruct { Type = EpsTaskType.OTC_DCL, Params = CreateDclParams(GetFileFormat(1), new string[] { "*", "*.frx" }, false, true) });
            _parallelParams.Add(new EpsTaskTypeStruct { Type = EpsTaskType.OTC_DCL, Params = CreateDclParams(GetFileFormat(2), new string[] { "*", "*.frx" }, false, true) });
            _parallelParams.Add(new EpsTaskTypeStruct
            {
                Type = EpsTaskType.OTC_SHARE,
                Params = CreateShareParams(GetFileFormat(3), EpsTaskProtect.CRC, new string[] { "*" }, false, true)
            });
            _parallelParams.Add(new EpsTaskTypeStruct
            {
                Type = EpsTaskType.OTC_SHARE,
                Params = CreateShareParams(GetFileFormat(4), EpsTaskProtect.EXT, new string[] { "*" }, false, true)
            });
            _parallelParams.Add(new EpsTaskTypeStruct
            {
                Type = EpsTaskType.OTC_SHARE,
                Params = CreateShareParams(GetFileFormat(5), EpsTaskProtect.FOLDER, new string[] { "*" }, false, true)
            });
            _parallelParams.Add(new EpsTaskTypeStruct
            {
                Type = EpsTaskType.OTC_FTP,
                Params = CreateFtpParams(GetFileFormat(6), 0, EpsTaskProtect.CRC, new string[] { "*" }, false, true)
            });
            _parallelParams.Add(new EpsTaskTypeStruct
            {
                Type = EpsTaskType.OTC_FTP,
                Params = CreateFtpParams(GetFileFormat(7), 1, EpsTaskProtect.EXT, new string[] { "*" }, false, true)
            });
            _parallelParams.Add(new EpsTaskTypeStruct
            {
                Type = EpsTaskType.OTC_FTP,
                Params = CreateFtpParams(GetFileFormat(8), 1, EpsTaskProtect.FOLDER, new string[] { "*" }, false, true)
            });
            _parallelParams.Add(new EpsTaskTypeStruct
            {
                Type = EpsTaskType.OTC_MAIL,
                Params = CreateMailParams(GetFileFormat(9), new string[] { Email }, new string[] { "*" }, 1, false, true)
            });
            _parallelParams.Add(new EpsTaskTypeStruct
            {
                Type = EpsTaskType.OTC_MAIL,
                Params = CreateMailParams(GetFileFormat(10), new string[] { Email }, new string[] { "*" }, 0, false, true)
            });
            _parallelParams.Add(new EpsTaskTypeStruct
            {
                Type = EpsTaskType.OTC_PRINT,
                Params = CreatePrintParams(GetFileFormat(11), 0, true)
            });
            _parallelParams.Add(new EpsTaskTypeStruct
            {
                Type = EpsTaskType.OTC_PRINT,
                Params = CreatePrintParams(GetFileFormat(12), 0, true)
            });
        }

        #endregion

        #region . Tests .
        //[Test]
        public Output CreateOutput()
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

            return CreateOutput(login_r: "EpsTest", epsHandler: EpsHandler, epsReports: epsReports,
                resultReportFile: resultReportFile, changeOdbc: null, useZip: true, useReserveCopy: true,
                outputTaskparams: new[] { CreateOutputTasks_OTC_MAIL(1), CreateOutputTasks_OTC_FTP(2, false, EpsTaskProtect.CRC) });
        }

        [Test]
        public void ValidParametersMatrixTest()
        {
            var status = EpsJobStatus.OS_COMPLETED;
            foreach (var key in _validParams)
            {
                var sts = DoOutput(new[] { CreateOutputTask(key.Type, 0, key.Params) });
                status = (sts == EpsJobStatus.OS_ERROR) ? EpsJobStatus.OS_ERROR : status;
            }
            status.ShouldBeEquivalentTo(EpsJobStatus.OS_COMPLETED);
        }

        [Test]
        public void InvalidParametersMatrixTest()
        {
            foreach (var key in _invalidParams)
            {
                Assert.Throws<AggregateException>(() => DoOutput(new[] { CreateOutputTask(key.Type, 0, key.Params) }));
            }
        }

        [Test]
        public void ParallelAllTask()
        {
            var status = EpsJobStatus.OS_COMPLETED;
            var taskList = new List<OutputTask>();
            taskList.Add(CreateOutputTask(EpsTaskType.OTC_DCL, 0, _validParams.FirstOrDefault(i => i.Type == EpsTaskType.OTC_DCL).Params));
            taskList.Add(CreateOutputTask(EpsTaskType.OTC_FTP, 0, _validParams.FirstOrDefault(i => i.Type == EpsTaskType.OTC_FTP).Params));
            taskList.Add(CreateOutputTask(EpsTaskType.OTC_SHARE, 0, _validParams.FirstOrDefault(i => i.Type == EpsTaskType.OTC_SHARE).Params));
            taskList.Add(CreateOutputTask(EpsTaskType.OTC_MAIL, 0, _validParams.FirstOrDefault(i => i.Type == EpsTaskType.OTC_MAIL).Params));
            taskList.Add(CreateOutputTask(EpsTaskType.OTC_PRINT, 0, _validParams.FirstOrDefault(i => i.Type == EpsTaskType.OTC_PRINT).Params));

            status = DoOutput(taskList.ToArray());

            status.ShouldBeEquivalentTo(EpsJobStatus.OS_COMPLETED);
        }

        [Test]
        public void ParallelDclTask()
        {
            var status = EpsJobStatus.OS_COMPLETED;
            status = ParallelT(EpsTaskType.OTC_DCL);
            status.ShouldBeEquivalentTo(EpsJobStatus.OS_COMPLETED);
        }

        [Test]
        public void ParallelShareTask()
        {
            var status = EpsJobStatus.OS_COMPLETED;
            status = ParallelT(EpsTaskType.OTC_SHARE);
            status.ShouldBeEquivalentTo(EpsJobStatus.OS_COMPLETED);
        }

        [Test]
        public void ParallelFtpTask()
        {
            var status = EpsJobStatus.OS_COMPLETED;
            status = ParallelT(EpsTaskType.OTC_FTP);
            status.ShouldBeEquivalentTo(EpsJobStatus.OS_COMPLETED);
        }

        [Test]
        public void ParallelMailTask()
        {
            var status = EpsJobStatus.OS_COMPLETED;
            status = ParallelT(EpsTaskType.OTC_MAIL);
            status.ShouldBeEquivalentTo(EpsJobStatus.OS_COMPLETED);
        }

        [Test]
        public void ParallelPrintTask()
        {
            var status = EpsJobStatus.OS_COMPLETED;
            status = ParallelT(EpsTaskType.OTC_PRINT);
            status.ShouldBeEquivalentTo(EpsJobStatus.OS_COMPLETED);
        }

        [Test]
        public void OrderedShareTask()
        {
            var status = EpsJobStatus.OS_COMPLETED;
            status = OrderedT(EpsTaskType.OTC_SHARE);
            status.ShouldBeEquivalentTo(EpsJobStatus.OS_COMPLETED);
        }

        [Test]
        public void OrderedFtpTask()
        {
            var status = EpsJobStatus.OS_COMPLETED;
            status = OrderedT(EpsTaskType.OTC_FTP);
            status.ShouldBeEquivalentTo(EpsJobStatus.OS_COMPLETED);
        }

        #endregion

        #region . Helpers .

        public EpsJobStatus ParallelT(EpsTaskType type)
        {
            var taskList = new List<OutputTask>();
            var taskParams = _parallelParams.Where(i => i.Type == type);

            foreach (var par in taskParams)
            {
                taskList.Add(CreateOutputTask(type, 0, par.Params));
            }

            return DoOutput(taskList.ToArray());
        }

        public EpsJobStatus OrderedT(EpsTaskType type)
        {
            var taskList = new List<OutputTask>();
            var taskParams = _validParams.Where(i => i.Type == type);
            var order = 0;
            foreach (var par in taskParams)
            {
                taskList.Add(CreateOutputTask(type, order++, par.Params));
            }

            return DoOutput(taskList.ToArray());
        }

        public EpsJobStatus DoOutput(OutputTask[] tasks, bool useZip = true, bool useReserveCopy = true)
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

            var output = CreateOutput(login_r: "EpsTest", epsHandler: EpsHandler, epsReports: epsReports,
                                      resultReportFile: resultReportFile, changeOdbc: null, useZip: useZip,
                                      useReserveCopy: useReserveCopy,
                                      outputTaskparams: tasks);

            //Запускаем EPS Job
            var epsjob = new wmsMLC.EPS.wmsEPS.EpsJob(output, "User");
            var task = new Task(epsjob.DoJob);
            task.Start();
            Trace.WriteLine(string.Format("Задание с данными {0} запущено.", EpsHelper.GetKey(epsjob.GetOutput())));
            Trace.WriteLine("======================================");
            task.Wait();

            using (var manager = IoC.Instance.Resolve<IBaseManager<Output>>())
            {
                manager.Update(output);
            }

            return output.OutputStatus.To<EpsJobStatus>();
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
                                               && epsTaskParam != EpsTaskParams.Conversion
                                               && epsTaskParam != EpsTaskParams.Spacelife
                                               && epsTaskParam != EpsTaskParams.BatchPrint

                                               //исключаем альтернативные параметры
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
                        item.Value = "0";
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
                        item.Value = @"TEST/INBOUND/TestSTF";
                        break;
                    case EpsTaskParams.TargetFolder:
                        item.Value = @"TEST/INBOUND/TestTF";
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

        public Output CreateOutput(string login_r, int epsHandler,
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
                /*case 13:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FROoBase";*/
                case 13:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FRPdf";
                case 14:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FRPowerPoint2007";
                case 15:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FRRtf";
                case 16:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FRText";
                case 17:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FRTiff";
                case 18:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FRPng";
                case 19:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FRWord2007";
                case 20:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FRXml";
                case 21:
                    return "wmsMLC.EPS.wmsEPS.ExportTypes.FRXps";
            }

            return "wmsMLC.EPS.wmsEPS.ExportTypes.FRPdf";
        }

        private IBaseManager<T> CreateManager<T>()
        {
            return IoC.Instance.Resolve<IBaseManager<T>>();
        }

        public OutputTask CreateOutputTask(EpsTaskType task, int outputTaskOrder, TaskOutputParam[] taskOutputParam)
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
                foreach (var p in taskOutputParam)
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
        #endregion
    }

    public class Variable
    {
        public string Value { get; set; }
        public string SubValue { get; set; }
    }

    public class TaskOutputParam
    {
        public EpsTaskParams Code { get; set; }
        public string Value { get; set; }
    }

    public class EpsTaskTypeStruct
    {
        public EpsTaskType Type;
        public TaskOutputParam[] Params;
    }
}
