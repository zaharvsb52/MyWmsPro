using System.Windows;
using DevExpress.Xpf.WindowsUI.Navigation;
using wmsMLC.RCL.Main.Helpers;

namespace wmsMLC.RCL.Main.Views
{
    public partial class InfoMenuView : INavigationAware
    {
        public InfoMenuView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            TileLayoutControlHelper.ScaleTransform(this, tileLayoutControl.ActualWidth, tileLayoutControl.ActualHeight);
        }

        #region . INavigationAware Members .
        void INavigationAware.NavigatedFrom(NavigationEventArgs e)
        {
        }

        void INavigationAware.NavigatedTo(NavigationEventArgs e)
        {
        }

        void INavigationAware.NavigatingFrom(NavigatingEventArgs e)
        {
            e.Cancel = Wait.IsBusy;
        }
        #endregion . INavigationAware Members .
    }
}
