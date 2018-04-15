using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using log4net;

namespace wmsMLC.General.Services.Service
{
    /// <summary>
    /// Запускатель приложения.
    /// Разбирает параметры, обрабатывает их и запускает приложение (app или service)
    /// </summary>
    public sealed class HostStarter
    {
        #region .  Const & Fields  .
        private const string ParamService = "service";
        private const string ParamServiceNo = "noservice";
        private const string Re = @"(?:-{1,2}|/)(?<name>\w+)(?:[=:]?|\s+)(?<value>[^-\s""][^""]*?|""[^""]*"")?(?=\s+[-/]|$)";

        private readonly string _defaultName;
        private readonly ILog _log;
        private readonly IHostFactory _hostFactory;
        #endregion

        public HostStarter(string defaultName, IHostFactory hostFactory)
        {
            if (string.IsNullOrEmpty(defaultName))
                throw new ArgumentNullException("defaultName");

            if (hostFactory == null)
                throw new ArgumentNullException("hostFactory");
            
            ExceptionPolicy.Instance.Init();

            _defaultName = defaultName;
            // раньше получать нельзя, т.к. не будет инициализрован
            _log = LogManager.GetLogger(GetType());
            _hostFactory = hostFactory;

            AppName = System.Diagnostics.Process.GetCurrentProcess().ProcessName.Replace(".vshost", "");
            ServiceName = AppName;
            DisplayName = AppName;
        }

        #region . Properties .
        public string AppName { get; private set; }
        public string ServiceName { get; private set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }

        public Dictionary<string, string> Parameters { get; private set; } 
        #endregion

        #region . Methods .

        public void Start(Type appType = null)
        {
            // если нет параметров - это ошибка
            if (Parameters == null || Parameters.Count == 0)
                throw new OperationException("Arguments is not defined. Please, check command line arguments");
           
            // применяем параметры
            if (Parameters.ContainsKey(ConfigBase.NameParam))
            {
                // вычитываем имя
                var name = Parameters[ConfigBase.NameParam];
                Log4NetHelper.SetServiceName(string.Format("_{0}", name));

                // Добавляем суффикс к имени сервиса
                ServiceName += ":" + name;
                DisplayName = ServiceName;
            }

            Log4NetHelper.Configure(_defaultName);
            // определяем комманду на удаление сервиса
            var isNeedRemoveService = Parameters.ContainsKey(ParamServiceNo);
            if (isNeedRemoveService)
            {
                RemoveService();
                return;
            }

            // определяем какой вид запуска (сервис или консоль)
            var runAsService = Parameters.ContainsKey(ParamService);

            var context = new ServiceContext(ServiceName, Parameters);

            //запуск приложения как сервиса
            if (runAsService)
            {
                // если сервиса нет - устанавливаем его и запускаем
                var sysService = GetInstalledService();
                if (sysService == null)
                {
                    sysService = CreateService();
                    sysService.Start();
                    return;
                }

                // если сервис есть - запускаем логику
                var svc = _hostFactory.GetService(context);
                ServiceBase.Run(svc);
            }
            //запуск в режиме консольного приложения
            else
            {
                IAppHost app = null;
                try
                {
                    app = _hostFactory.GetApp(context);
                    app.Start(null);

                    _log.Info("Press escape to exit");
                    ConsoleKeyInfo keyInfo;
                    do keyInfo = Console.ReadKey();
                    while (keyInfo.Key != ConsoleKey.Escape);
                }
                catch (Exception ex)
                {
                    _log.Error("Fatal error." + ExceptionHelper.ExceptionToString(ex), ex);
                }
                finally
                {
                    if (app != null)
                        app.Stop();
                }
            }
        }

        public void SetParametersFromCommandLine(string commandLine)
        {
            Parameters = ParseCommandLine(commandLine);
        }

        private ServiceController GetInstalledService()
        {
            return ServiceController.GetServices().FirstOrDefault(con => con.ServiceName == ServiceName);
        }

        private ServiceController CreateService()
        {
            var service = ServiceController.GetServices().FirstOrDefault(con => con.ServiceName == ServiceName);
            if (service != null)
                throw new DeveloperException("Service '{0}' already installed", ServiceName);

            _log.DebugFormat("Try to install service '{0}'", ServiceName);

            var parameters = string.Join(" ", Parameters.Select(i => "-" + i.Key + (i.Value != null ? "=" + i.Value : string.Empty)));
            var appPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var svcPath = string.Format("\"{0}\\{1}.exe\" {2}", appPath, AppName, parameters);
            if (!DirectServiceInstaller.InstallService(svcPath, ServiceName, DisplayName, Description, true, false))
                throw new DeveloperException("Can't install service. Check user rights for this operation.");

            // проверяем, что сервис установился
            service = ServiceController.GetServices().FirstOrDefault(con => con.ServiceName == ServiceName);
            if (service == null)
                throw new DeveloperException("Can't find just installed service " + ServiceName);

            _log.DebugFormat("Service '{0}' successfully installed", ServiceName);
            return service;
        }

        private void RemoveService()
        {
            var service = ServiceController.GetServices().FirstOrDefault(con => con.ServiceName == ServiceName);
            if (service == null)
            {
                _log.WarnFormat("Can't stop service '{0}'. Service not found", ServiceName);
                return;
            }

            // если работает - останавливаем
            if (service.Status == ServiceControllerStatus.Running)
                service.Stop();

            // удаляем
            _log.InfoFormat(
                DirectServiceInstaller.UnInstallService(ServiceName)
                    ? "Service '{0}' successfully removed"
                    : "Can't remove '{0}' service. Unknown error in ServiceInstaller", ServiceName);
        }

        private static Dictionary<string, string> ParseCommandLine(string commandLine)
        {
            var ms = Regex.Matches(commandLine, Re);
            var matches = ms.Cast<Match>()
                .Select(m => m.Groups[1].Value + (m.Groups[2].Success ? "=" + Regex.Replace(
                    m.Groups[2].Value, @"""", string.Empty) : string.Empty)).ToArray();

            return matches.ToDictionary(i => i.Split('=')[0], i => i.Split('=').Length > 1 ? i.Split('=')[1] : null);
        }

        #endregion
    }
}
