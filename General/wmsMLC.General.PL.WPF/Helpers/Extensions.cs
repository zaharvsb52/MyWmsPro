using System;
using System.Windows;
using System.Windows.Threading;

namespace wmsMLC.General.PL.WPF.Helpers
{
    public static class Extensions
    {
        public static void BackgroundFocus(this UIElement element)
        {
            if (element == null)
                return;
            Action a = () => element.Focus();
            element.Dispatcher.BeginInvoke(DispatcherPriority.Background, a);
        }
    }
}
