using System.Linq;
using System.Windows;
using System.Windows.Input;
using DevExpress.Xpf.Bars;

namespace wmsMLC.General.PL.WPF.Components.Controls.Rcl
{
    public class CustomBarManager : BarManager
    {
        protected override void OnLoaded(object sender, System.EventArgs e)
        {
            base.OnLoaded(sender, e);
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.PreviewKeyDown -= OnWindowPreviewKeyDown;
                window.PreviewKeyDown += OnWindowPreviewKeyDown;
            }
        }

        private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.System)
            //{
            //    e.Handled = true;
            //    return;
            //}

            var window = sender as Window;
            if (window == null)
                return;

            var keys = GetItemsKey();
            if (keys.Any(key => key.Matches(window, e)))
                return;
            var element = FocusManager.GetFocusedElement(window) as FrameworkElement;
            if (element == null)
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private KeyGesture[] GetItemsKey()
        {
            return Items.Where(p => p.KeyGesture != null).Select(p => p.KeyGesture).ToArray();
        }
    }

    public class CustomBar : Bar
    {
    }

    public class CustomBarButtonItem : BarButtonItem
    {
    }
}
