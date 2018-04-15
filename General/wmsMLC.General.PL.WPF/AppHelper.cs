using System;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Media.Animation;
using log4net;

namespace wmsMLC.General.PL.WPF
{
    public static class AppHelper
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(AppHelper));

        public static ILog Log { get { return _log; } }

        public static void Init(string appName)
        {
            Log4NetHelper.Configure(appName);

            // код ниже позволяет отследить незагруженные сборки
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;

            try
            {
                // снижаем частоту обновления картинки на экране (должно помочь на слабых процессорах)
                Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline),
                    new FrameworkPropertyMetadata { DefaultValue = 10 });

                PrintStartLogHeader();
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
            }
        }

        public static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            // в лог выводим только, если не ресурсы
            if (!args.Name.Contains(".resources"))
            {
                _log.WarnFormat("Can't resolve assembly with name '{0}'.", args.Name);
            }
            return null;
        }

        public static void PrintStartLogHeader()
        {
            var sb = new StringBuilder("===============================", 1000);
            sb.AppendLine("Start application");
            sb.AppendLine(string.Format(@"User: {0}\{1}", Environment.UserDomainName, Environment.UserName));
            sb.AppendLine(string.Format(@"Machine: {0}", Environment.MachineName));
            sb.AppendLine(string.Format(@"OS: {0} ({1})", Environment.OSVersion.VersionString, Environment.Is64BitOperatingSystem ? "x64" : "x86"));
            sb.AppendLine(string.Format(@"Current dir:" + Environment.CurrentDirectory));
            sb.AppendLine("===============================");
            _log.Info(sb.ToString());
        }

        public static void PrintStopLogHeader()
        {
            var sb = new StringBuilder("===============================", 1000);
            sb.AppendLine("===============================");
            sb.AppendLine("Stop application");
            sb.AppendLine("===============================");
            _log.Info(sb.ToString());
        }

        public static void InitExceptionEngine(Application application)
        {
            ExceptionPolicy.Instance.Init();
            ExceptionPolicy.Instance.ExceptionOccure += HandleException;

            // подписываемся на UI Exception-ы
            application.DispatcherUnhandledException += (sender, args) =>
            {
                args.Handled = ExceptionPolicy.Instance.HandleException(args.Exception, application.GetType());
            };
        }

        public static void HandleException(object sender, ExceptionEventArgs e)
        {
            // логируем все
            _log.Error(ExceptionHelper.ExceptionToString(e.Exception), e.Exception);

            // обрабатываем политики - ошибки уровня App не должны проваливаться никуда дальше
            if (!e.Handled)
            {
                e.NeedThrow = false;
                e.Handled = true;
            }
        }
    }
}