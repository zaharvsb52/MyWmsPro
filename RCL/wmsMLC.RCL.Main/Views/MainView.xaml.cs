using System;
using System.Windows;
using wmsMLC.General;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.RCL.Main.ViewModels;
using wmsMLC.RCL.Resources;

namespace wmsMLC.RCL.Main.Views
{
    public partial class MainView
    {
        private MainViewModel _mainViewModel;

        public MainView()
        {
            //Не трогать !!!!!!
            DataContextChanged += (s, e) =>
            {
                if (e.NewValue == null)
                    return;

                _mainViewModel = (MainViewModel)e.NewValue;
            };

            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnLoaded;

            var env = WMSEnvironment.Instance.DbSystemInfo.Environment;
            if (!string.IsNullOrEmpty(env))
                ErrorBox.AddAdditionalParam(BugReportResources.Environment, env);

            var ver = WMSEnvironment.Instance.DbSystemInfo.Version;
            if (!string.IsNullOrEmpty(ver))
                ErrorBox.AddAdditionalParam(BugReportResources.VersionDB, ver);

            var sdcl = WMSEnvironment.Instance.SdclCode;
            if (!string.IsNullOrEmpty(sdcl))
                ErrorBox.AddAdditionalParam(BugReportResources.SdclCode, sdcl);

            var endpoint = WMSEnvironment.Instance.EndPoint;
            if (!string.IsNullOrEmpty(endpoint))
                ErrorBox.AddAdditionalParam(BugReportResources.SDCL_Endpoint, endpoint);

            if (_mainViewModel == null)
                return;

            if (_mainViewModel.MainTileMenu == null)
                _mainViewModel.ShowMainMenu();

            //Если есть критические ошибки показываем и выходим из приложения
            if (_mainViewModel.HasCriticalErrors)
            {
                var viewService = IoC.Instance.Resolve<IViewService>();
                viewService.ShowDialog(RCL.Resources.StringResources.RCL
                , _mainViewModel.GetCriticalErrorMessage()
                , MessageBoxButton.OK
                , MessageBoxImage.Error
                , MessageBoxResult.OK);

                Application.Current.MainWindow.Close();
                Environment.Exit(0);
            }
        }

        private void OnContextMenuMenuItemClick(object sender, RoutedEventArgs e)
        {
            //TODO: это жесткий ХАК, надо переделать на нормальный биндинг
            var mvm = DataContext as MainViewModel;
            if (mvm != null) 
                mvm.SystemMessageCommand.Execute(null);
        }
    }
}
