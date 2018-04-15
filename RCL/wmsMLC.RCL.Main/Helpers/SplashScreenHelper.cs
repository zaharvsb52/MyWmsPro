using System.Globalization;
using DevExpress.Xpf.Core;
using wmsMLC.RCL.Main.Views;

namespace wmsMLC.RCL.Main.Helpers
{
    public static class SplashScreenHelper
    {
        private static volatile SplashScreenView _splashScreen;

        public static void Show(CultureInfo culture)
        {
            _splashScreen = null;
            DXSplashScreen.Show(cu =>
            {
                _splashScreen = new SplashScreenView((CultureInfo) cu);
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
            }
        }
    }
}
