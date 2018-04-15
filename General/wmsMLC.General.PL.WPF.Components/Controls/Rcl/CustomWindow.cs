using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using DevExpress.Xpf.Core;

namespace wmsMLC.General.PL.WPF.Components.Controls.Rcl
{
    public class CustomWindow : DXWindow
    {
        public CustomWindow()
        {
            Padding = new Thickness(0, 0, 0, 0);
            ShowIcon = false;
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;
            ExitByEscape = true;

            if (ThemeManager.ApplicationThemeName != string.Empty)
                SetValue(ThemeManager.ThemeNameProperty, ThemeManager.ApplicationThemeName);

            Loaded += OnLoaded;
        }

        #region . Properties .
        public bool ExitByEscape { get; set; }
        #endregion . Properties .

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var title = GetVisualByName("PART_TITLE") as TextBlock;
            if (title == null)
                return;

            Loaded -= OnLoaded;
            title.SetBinding(TextBlock.TextProperty, new Binding("Title") { Source = this });
            title.SetBinding(VisibilityProperty,
                new Binding("ShowTitle")
                {
                    Source = this,
                    Converter = new BoolToVisibilityConverter()
                });
        }

        protected virtual void OnWindowClose(KeyEventArgs e)
        {
            if (!ExitByEscape || e.Handled)
                return;

            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            //Используем только для выхода по Esc
            if(!e.Handled && e.Key == Key.Escape)
                OnWindowClose(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            OnWindowClose(e);
            base.OnKeyDown(e);
        }
    }
}
