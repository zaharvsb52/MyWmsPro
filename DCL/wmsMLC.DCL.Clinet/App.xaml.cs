using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Helpers;
using WPFLocalizeExtension.Engine;
using wmsMLC.DCL.Main.Helpers;
using wmsMLC.General;
using wmsMLC.General.Updater;

namespace wmsMLC.DCL.Client
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppHelper.Init(ClientTypeCode.DCL.ToString());

#if !DEBUG
            CheckForUpdate();
#endif

            try
            {
                LocalizeDictionary.Instance.Culture = Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = Client.Properties.Settings.Default.Culture;
                SplashScreenHelper.Show(Thread.CurrentThread.CurrentCulture);
                SplashScreenHelper.SetState(DCL.Resources.StringResources.StateInitModules);

#if DEBUG
                // HACK: надоело по сто раз перегружаться
                SplashScreenHelper.SetState(DCL.Resources.StringResources.WaitServiceInit);
                //Thread.Sleep(10000);
#endif

                AppHelper.InitExceptionEngine(this);

                base.OnStartup(e);

                // стартуем "движок"
                var bootstrapper = new Bootstraper();
                bootstrapper.Run();

                // авторизуемся
                SplashScreenHelper.SetState(DCL.Resources.StringResources.StateAuthentication);
                AuthenticationHelper.AskCredentials = true;

                while (true)
                {
                    try
                    {
                        if (!Authenticate())
                        {
                            AppHelper.Log.Info("Exit from application. User was not Authenticated");
                            Environment.Exit(0);
                        }
                        else
                            // идем дальше
                            break;
                    }
                    catch (Exception ex)
                    {
                        if (ex is EndpointNotFoundException || ex.InnerException is General.CommunicationException)
                        {
                            if (
                                MessageBox.Show("Отсутствует связь с сервисом.\r\nПовторить попытку подключения?",
                                    "Соединение", MessageBoxButton.YesNo,
                                    MessageBoxImage.Question,
                                    MessageBoxResult.Yes) == MessageBoxResult.No)
                            {
                                AppHelper.Log.Info("Не удалось соединиться с сервисом");
                                Environment.Exit(0);
                            }
                        }
                        else
                            throw;
                    }
                }

                // запускаем модули
                SplashScreenHelper.SetState(DCL.Resources.StringResources.StateRunModules);
                bootstrapper.RunModules();

                // дополнительная проверка после общего запуска
                SplashScreenHelper.SetState(DCL.Resources.StringResources.StateRun);
                if (AuthenticationHelper.IsAuthenticated)
                {
                    AppHelper.Log.Debug("Show main form");
                    Current.MainWindow.Closed += MainWindow_Closed;
                    Current.MainWindow.UpdateLayout();
                    Current.MainWindow.Show();
                    SignalRHelper.TryConnectToServer();
                }
                else
                {
                    AppHelper.Log.Info("Exit from application. User Authentication was canceled");
                    Current.Shutdown();
                }

                SplashScreenHelper.Close();
            }
            catch (Exception ex)
            {
                SplashScreenHelper.Close();

                // обрабатываем ошибку
                if (!ExceptionPolicy.Instance.HandleException(ex, typeof(App)))
                    throw;

                // и безусловно выходим из приложения (без инициализации работать не можем)
                AppHelper.Log.Info("Exit from application. Exception was not handled");
                Environment.Exit(0);
            }
            finally
            {
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
//            if (AuthenticationHelper.Authenticate(DCL.Resources.StringResources.DEBUG, DCL.Resources.StringResources.DEBUG))
//                return true;
//            return false;
//#else
            return AuthenticationHelper.Authenticate(SplashScreenHelper.SplashScreenHandle);
//#endif
        }

        private void CheckForUpdate()
        {
            bool isCritical = true;

            try
            {
                AppHelper.Log.Info("Проверка обновлений");

                bool isCantCopyToLocal;
                var updateInfo = UpdateHelper.ReadUpdateInfo(Environment.CurrentDirectory, Client.Properties.Settings.Default.UpdateInfoFile, out isCantCopyToLocal);

                if (updateInfo.Updating)
                {
                    MessageBox.Show(
                        "В данный момент выполняется обновление системы.\r\nПовторите попытку позже.", "ВНИМАНИЕ!", MessageBoxButton.OK, MessageBoxImage.Information);
                    Shutdown();
                    return;
                }

                var isRunUpdate = UpdateHelper.CheckForUpdate(AssemblyAttributeAccessors.AssemblyFileVersion, updateInfo.LastVersion, updateInfo.MinimalSupportVersion, out isCritical);
                if (!isRunUpdate)
                {
                    AppHelper.Log.Info("Версия актуальна");
                    return;
                }

                AppHelper.Log.DebugFormat("Предлагаем обновиться до версии {0}. Минимальная поддерживаемая {1}", updateInfo.LastVersion, updateInfo.MinimalSupportVersion);

                if (!isCritical)
                {
                    var result = MessageBox.Show("Доступна новая версия приложения. Хотите обновить?", "Обновление",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.No)
                    {
                        AppHelper.Log.Info("Пользователь отказался от обновления");
                        isRunUpdate = false;
                    }
                }
                else
                {
                    AppHelper.Log.Debug("Критическое обновление. Применяем без запроса пользрвателя");
                }

                if (!isRunUpdate)
                    return;

                if (isCantCopyToLocal)
                {
                    throw new Exception("Нет доступа к локальной директории");
                }

                AppHelper.Log.Info("Запускаем процесс обновления");
                var updateTool = UpdateHelper.GetUpdater(Environment.CurrentDirectory, updateInfo.UpdateTool);
                UpdateHelper.RunUpdater(updateTool, Client.Properties.Settings.Default.UpdateInfoFile);

                AppHelper.Log.Info("Завершаем приложение");
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                AppHelper.Log.WarnFormat("Ошибка проверки обновлений. {0}", ex.Message);
                AppHelper.Log.Debug(ex);

                if (isCritical)
                {
                    MessageBox.Show(
                        "Возникла ошибка при обновлении!" + Environment.NewLine + ex.Message + Environment.NewLine + "Обратитесь в Службу поддержки.", "Ошибка при обновлении", MessageBoxButton.OK, MessageBoxImage.Error);

                    //ErrorBox.ShowError("Exception: ", ex);

                    Environment.Exit(0);
                }
            }
        }

        private static void MainWindow_Closed(object sender, EventArgs e)
        {
            if (Current.MainWindow != null)
                Current.MainWindow.Closed -= MainWindow_Closed;

            // пытаемся корректно завершить сессию работы
            // делаем это в отдельном потоке, чтобы выставить timeout и гарантированно закрыться
            try
            {
                using (var logOffTask = new Task(AuthenticationHelper.LogOff))
                {
                    logOffTask.Start();
                    logOffTask.Wait(new TimeSpan(0, 0, 0, 10));
                }
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