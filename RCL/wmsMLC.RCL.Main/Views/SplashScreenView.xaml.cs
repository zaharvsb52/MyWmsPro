using System;
using System.Globalization;
using System.Threading;
using System.Windows.Threading;
using DevExpress.Xpf.Core;
using wmsMLC.RCL.Resources;

namespace wmsMLC.RCL.Main.Views
{
    public partial class SplashScreenView : ISplashScreen
    {
        public SplashScreenView(CultureInfo culture)
        {
            InitializeComponent();
#if DEBUG
            Topmost = false;
#endif

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

        void ISplashScreen.SetProgressState(bool isIndeterminate)
        {
        }
        #endregion ISplashScreen
    }
}
