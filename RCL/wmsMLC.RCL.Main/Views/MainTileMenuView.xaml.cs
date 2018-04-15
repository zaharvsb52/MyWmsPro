using System;
using System.Windows;
using System.Windows.Input;
using DevExpress.Xpf.WindowsUI.Navigation;
using wmsMLC.General;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.RCL.Main.Helpers;
using wmsMLC.RCL.Main.ViewModels;
using wmsMLC.RCL.Resources;

namespace wmsMLC.RCL.Main.Views
{
    public partial class MainTileMenuView : INavigationAware
    {
        private bool _isNavigated;

        public MainTileMenuView()
        {
            InitializeComponent();

            Loaded += OnLoaded;
            DataContextChanged += OnDataContextChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;

            TileLayoutControlHelper.ScaleTransform(this, tileLayoutControl.ActualWidth, tileLayoutControl.ActualHeight);

            var form = Application.Current.MainWindow;
            if (form != null)
                form.KeyDown += OnKeyDownMainForm;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;

            var model = (MainViewModel)e.NewValue;
            if (model.MainTileMenu == null)
                model.ShowMainMenu();
        }

        private void OnKeyDownMainForm(object sender, KeyEventArgs e)
        {
            if (!Wait.IsBusy && !_isNavigated && !e.Handled && e.Key == Key.Escape)
            {
                e.Handled = true;
                if (GetViewService().ShowDialog(StringResources.RCL
                    , string.Format(StringResources.ConfirmExit, null, null)
                    , MessageBoxButton.OKCancel
                    , MessageBoxImage.Question
                    , MessageBoxResult.OK) == MessageBoxResult.OK)
                {
                    Application.Current.MainWindow.Close();
                    Environment.Exit(0);
                }
            }
        }

        private IViewService GetViewService()
        {
            return IoC.Instance.Resolve<IViewService>();
        }
        
        #region . INavigationAware Members .
        void INavigationAware.NavigatedFrom(NavigationEventArgs e)
        {

        }

        void INavigationAware.NavigatedTo(NavigationEventArgs e)
        {
            _isNavigated = false;
        }

        void INavigationAware.NavigatingFrom(NavigatingEventArgs e)
        {
            if (Wait.IsBusy)
            {
                e.Cancel = true;
                return;
            }
            _isNavigated = true;
        }
        #endregion . INavigationAware Members .
    }
}
