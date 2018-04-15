using System;
using System.Globalization;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Helpers;
using WPFLocalizeExtension.Engine;
using wmsMLC.RCL.Main.Helpers;
using wmsMLC.General;
using wmsMLC.General.Updater;
using CommunicationException = wmsMLC.General.CommunicationException;

namespace wmsMLC.RCL.Client
{
    public partial class App
    {
        private const int MaxCountConnectionAttempt = 7; //Достаточно случайное число

        protected override void OnStartup(StartupEventArgs e)
        {
            AppHelper.Init(ClientTypeCode.RCL.ToString());

#if !DEBUG
            AppHelper.Log.Info("Проверка обновлений");

            bool isCantCopyToLocal;
            var updateInfo = UpdateHelper.ReadUpdateInfo(Environment.CurrentDirectory, Client.Properties.Settings.Default.UpdateInfoFile, out isCantCopyToLocal);

            if (updateInfo == null)
            {
                MessageBox.Show(
                    "В данный момент выполняется обновление системы.\r\nПовторите попытку позже.", "ВНИМАНИЕ!", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
                return;
            }
#endif

            try
            {
                CultureInfo.DefaultThreadCurrentCulture =
                    LocalizeDictionary.Instance.Culture =
                        Thread.CurrentThread.CurrentCulture =
                            Thread.CurrentThread.CurrentUICulture = Client.Properties.Settings.Default.Culture;

                SplashScreenHelper.Show(Thread.CurrentThread.CurrentCulture);
                SplashScreenHelper.SetState(RCL.Resources.StringResources.StateInitModules);
                AppHelper.InitExceptionEngine(this);

                base.OnStartup(e);

                // стартуем "движок"
                var bootstrapper = new Bootstraper();
                bootstrapper.Run();

#if DEBUG
                SplashScreenHelper.SetState(RCL.Resources.StringResources.WaitServiceInit);
                Thread.Sleep(5000);
#endif

                // запускаем модули
                SplashScreenHelper.SetState(RCL.Resources.StringResources.StateRunModules);
                bootstrapper.RunModules();

                // авторизуемся
                SplashScreenHelper.SetState(RCL.Resources.StringResources.StateAuthentication);
                AuthenticationHelper.AskCredentials = true;

                var count = 0;
                while (true)
                {
                    try
                    {
                        if (!Authenticate())
                        {
                            AppHelper.Log.Info("Exit from application. User was not Authenticated.");
                            Environment.Exit(0);
                        }
                        else
                            // идем дальше
                            break;
                    }
                    catch (Exception ex)
                    {
                        if (ex is EndpointNotFoundException || ex.InnerException is CommunicationException)
                        {
                            if (count++ < MaxCountConnectionAttempt)
                            {
                                AppHelper.Log.Info(string.Format("Попытка соединения с сервисом {0}", count));
                            }
                            else
                            {
                                AppHelper.Log.Info("Не удалось соединиться с сервисом");
                                throw new DeveloperException("Превышено количество попыток соединения с сервером. Приложение будет закрыто.", ex);
                                //Environment.Exit(0);
                            }
                        }
                        else
                            throw;
                    }
                }

                // дополнительная проверка после общего запуска
                SplashScreenHelper.SetState(RCL.Resources.StringResources.StateRun);
                if (AuthenticationHelper.IsAuthenticated)
                {
                    AppHelper.Log.Debug("Show main form");
                    Current.MainWindow.Closed += MainWindow_Closed;
                    Current.MainWindow.UpdateLayout();
                    Current.MainWindow.Show();
                }
                else
                {
                    AppHelper.Log.Info("Exit from application. User Authentication was canceled");
                    Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                // обрабатываем ошибку
                if (!ExceptionPolicy.Instance.HandleException(ex, typeof(App)))
                    throw;

                // и безусловно выходим из приложения (без инициализации работать не можем)
                AppHelper.Log.Info("Exit from application. Exception was not handled");
                Environment.Exit(0);
            }
            finally
            {
                SplashScreenHelper.Close();
                AppHelper.Log.Debug("Startup complete");
            }
        }

        private static bool Authenticate()
        {
//#if DEBUG
//            // пробуем под собой
//            if (AuthenticationHelper.Authenticate(Environment.UserName.ToUpper(), "123"))
//                return true;
//            // пробуем под DEBUG
//            if (AuthenticationHelper.Authenticate(RCL.Resources.StringResources.DEBUG, RCL.Resources.StringResources.DEBUG))
//                return true;
//            return false;
//#else
            return AuthenticationHelper.Authenticate();
//#endif
        }

        private static void MainWindow_Closed(object sender, EventArgs e)
        {
            if (Current.MainWindow != null)
                Current.MainWindow.Closed -= MainWindow_Closed;

            // пытаемся корректно завершить сессию работы
            // делаем это в отдельном потоке, чтобы выставить timeout и гарантированно закрыться
            try
            {
                var logOffTask = new Task(AuthenticationHelper.LogOff);
                logOffTask.Start();
                logOffTask.Wait(new TimeSpan(0, 0, 0, 10));
            }
            catch (Exception ex)
            {
                AppHelper.Log.Error("Ошибка завершения сеанса.", ex);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            AppHelper.PrintStopLogHeader();
            AppHelper.Log.Debug("Завершение сеанса.");
            base.OnExit(e);
        }
    }
}
