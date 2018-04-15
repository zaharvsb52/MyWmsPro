using System;
using System.Windows;
using EventTrigger = System.Windows.Interactivity.EventTrigger;

namespace wmsMLC.DCL.Main.Views.Controls
{
    public class HandledEventTrigger : EventTrigger
    {
        protected override void OnEvent(EventArgs e)
        {
            var args = e as RoutedEventArgs;
            if (args != null)
                args.Handled = true;
            base.OnEvent(e);
        }
    }
}
