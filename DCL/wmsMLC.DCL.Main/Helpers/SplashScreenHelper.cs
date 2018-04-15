using System;
using System.Globalization;
using System.Windows.Interop;
using DevExpress.Xpf.Core;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General;

namespace wmsMLC.DCL.Main.Helpers
{
    public static class SplashScreenHelper
    {
        private static volatile SplashScreenView _splashScreen;
        public static IntPtr? SplashScreenHandle { get; private set; }

        public static void Show(CultureInfo culture)
        {
            if (_splashScreen != null)
                throw new DeveloperException("Пердыдущий SplashScreen не был закрыт.");

            _splashScreen = null;
            DXSplashScreen.Show(cu =>
            {
                _splashScreen = new SplashScreenView((CultureInfo)cu);

                try
                {
                    SplashScreenHandle = _splashScreen == null
                        ? (IntPtr?) null
                        : new WindowInteropHelper(_splashScreen).EnsureHandle();
                }
                catch
                {
                    SplashScreenHandle = null;
                }

                return _splashScreen;
            }, null, culture, null);
        }

        public static void Progress(double value)
        {
            DXSplashScreen.Progress(value);
        }

        public static void SetState(string value)
        {
            if (_splashScreen != null)
                _splashScreen.SetState(value);
        }

        public static void Close()
        {
            try
            {
                DXSplashScreen.Close();
            }
            finally
            {
                _splashScreen = null;
                SplashScreenHandle = null;
            }
        }
    }
}
