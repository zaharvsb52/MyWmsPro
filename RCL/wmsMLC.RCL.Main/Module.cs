using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using log4net;
using Microsoft.Practices.Unity;
using wmsMLC.Business;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Components.Helpers;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.Views;
using wmsMLC.RCL.Main.Properties;
using wmsMLC.RCL.Main.Services;
using wmsMLC.RCL.Main.ViewModels;
using wmsMLC.RCL.Main.Views;

namespace wmsMLC.RCL.Main
{
    public sealed class Module : ModuleBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Module));

        public Module(IUnityContainer container) : base(container) { }

        public override void Run()
        {
            base.Run();

            UiCore.ReturnedScannerKey = Settings.Default.ReturnedScannerKey.To(Key.Enter);
            UiCore.NextControlKey = Settings.Default.NextControlKey.To(Key.Enter);

            InitDevexpress();
            //LoadApplicationResource("/wmsMLC.RCL.Main;Component/Themes/Generic.xaml");
            LoadApplicationResource("/wmsMLC.General.PL.WPF.Components;Component/Themes/Generic.xaml");

            InitProcessAttributeStrategies();

            InitValidateStrategies();

            //Подпишем кого надо на события
            BLHelper.SubscribeEvents();
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            //Настраиваем Business Layer (Пока тут)
            BLHelper.InitBL(new IoCConfigurationContext { ExternalContainer = Container }, DALType.Service);

            //Регистрируем клиента для сервиса
            var clienttypecode = Settings.Default.ClientType.To(ClientTypeCode.None);
            if (clienttypecode == ClientTypeCode.None)
                throw new DeveloperException("ClientType '{0}' is undefined.", clienttypecode);
            BLHelper.RegisterServiceClient(Settings.Default.SessionId, clienttypecode, Settings.Default.SDCL_Endpoint);

            // подписываемся на обработку ошибок
            ExceptionPolicy.Instance.ExceptionOccure += OnExceptionOccure;

            // регистрируем Shell
            Container.RegisterType<Shell>(new ContainerControlledLifetimeManager()).RegisterType<IShell, Shell>();

            // регистрируем сервисы
            Container.RegisterType<IViewService, ViewService>(new ContainerControlledLifetimeManager());

            // регистрируем главную форму
            Container.RegisterInstance(typeof(MainView), Container.Resolve<MainView>(), new ContainerControlledLifetimeManager());

            // стартуем Caliburn
            var caliburnBootstrapper = new CaliburnBootstrapper(Container);
            caliburnBootstrapper.Start();

            IoC.Instance.Register<IAuthenticationViewModel, AuthenticationViewModel>();
        }

        private static void InitDevexpress()
        {
            var theme = Settings.Default.ApplicationThemeName.GetTrim();
            if (string.IsNullOrEmpty(theme))
                theme = StyleKeys.RclDefaultThemeName;
            else if (!StyleKeys.RclDefaultThemeName.EqIgnoreCase(theme) && !Theme.TouchlineDarkName.EqIgnoreCase(theme))
                theme = StyleKeys.RclDefaultThemeName;
            ThemeManager.ApplicationThemeName = theme;
            //ThemeManager.ApplicationThemeName = "MetropolisLight";
            //ThemeManager.ApplicationThemeName = "TouchlineDark";
            
            DataControlBase.AllowInfiniteGridSize = true;
        }

        private static void OnExceptionOccure(object sender, ExceptionEventArgs e)
        {
            if (e.PolicyName == BlExceptionHandler.BusinessLogicPolicyName)
                return; //В лог записали. Должно сработать OperationException

            // для процессов такие ошибки нужно пробрасывать до пользователя
            if (e.PolicyName == "BP" && e.Exception is PassThroughException)
            {
                e.Handled = true;
                e.NeedThrow = true;
                return;
            }

            try
            {
                // обрабатываем (последний рубеж)
                Log.DebugFormat("Start showing exception message");

                //INFO: пробуем сделать все правильно и поживем вот так
                // если все ок, то закоментированный код удалить
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    Log.DebugFormat("Main window is not loaded. Show new error window");
                    ShowError(e.Exception);
                }
                else
                {
                    Log.DebugFormat("Main window is loaded. Trying to show throw dispatcher");
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() =>
                    {
                        try
                        {
                            ShowError(e.Exception);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex);
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                Log.FatalFormat("!!! === EXCEPTION WAS NOT SHOWN === !!!");
                Log.Debug(ex);
            }

            e.NeedThrow = false;
            e.Handled = true;

            #region Старый код. Вызывал deadlock в случае ошибки при запуске activity из ExecuteWorkflowActivity
            //if (e.PolicyName == "BusinessLogicPolicy" || e.PolicyName == "RclBackgroundProcess")  //В лог записали. Должно сработать OperationException
            //    return;
            //if (Application.Current.Dispatcher.CheckAccess())
            //{
            //    Log.DebugFormat("Main window is not loaded. Show new error window");
            //    ErrorBox.ShowError("Exception: ", e.Exception);
            //}
            //else
            //{
            //    Log.DebugFormat("Main window is loaded. Trying to show throw dispatcher");
            //    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() => ErrorBox.ShowError("Exception: ", e.Exception)));
            //}
            #endregion Старый код
        }

        private static void ShowError(Exception ex)
        {
            ErrorBox.ShowError("Exception: ", ex);
        }

        /// <summary>
        /// Метод добавляющий стратегии формтирования аттрибутов сущности
        /// </summary>
        private static void InitProcessAttributeStrategies()
        {
            ProcessAttributeStrategiesHelper.Initialize();
        }

        public static void InitValidateStrategies()
        {
            ValidateStrategiesHelper.Initialize();
        }
    }
}