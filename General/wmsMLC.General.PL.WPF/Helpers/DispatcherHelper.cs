using System;
using System.Windows;
using System.Windows.Threading;

namespace wmsMLC.General.PL.WPF.Helpers
{
    public static class DispatcherHelper
    {
        public static void Invoke(Delegate action)
        {
            if (Application.Current == null || Application.Current.Dispatcher.CheckAccess())
            {
                action.DynamicInvoke(null);
            }
            else
            {
                Application.Current.Dispatcher.Invoke(action);
            }
        }

        public static void BeginInvoke(Delegate action)
        {
            if (Application.Current == null || Application.Current.Dispatcher.CheckAccess())
            {
                action.DynamicInvoke(null);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(action);
            }
        }

        public static void BeginInvoke(Action action, DispatcherPriority dispatcherPriority)
        {
            if (Application.Current == null || Application.Current.Dispatcher.CheckAccess())
            {
                action.DynamicInvoke(null);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(action, dispatcherPriority);
            }
        }
    }
}
