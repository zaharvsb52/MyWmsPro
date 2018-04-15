using System;
using System.Globalization;
using System.Threading;
using System.Windows.Threading;
using DevExpress.Xpf.Core;
using wmsMLC.DCL.Resources;

namespace wmsMLC.DCL.Main.Views
{
    public partial class SplashScreenView : ISplashScreen
    {
        public SplashScreenView(CultureInfo culture)
        {
            InitializeComponent();

            //DataContext = new SplashScreenViewModel();

            Thread.CurrentThread.CurrentCulture =
                Thread.CurrentThread.CurrentUICulture = culture;
        }

        public void SetState(string stateName)
        {
            Dispatcher.BeginInvoke(
                (Action) (() => { Info.Text = string.Format(StringResources.StatePrefix, stateName); }),
                DispatcherPriority.Loaded);
        }

        #region ISplashScreen
        public void Progress(double value)
        {
            progressBar.Value = value;
        }

        public void CloseSplashScreen()
        {
            Close();
        }

        public void SetProgressState(bool isIndeterminate)
        {
        }
        #endregion ISplashScreen
    }
}
