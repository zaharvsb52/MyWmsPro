using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using wmsMLC.General.PL.WPF.Components.Helpers;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.General.PL.WPF.Components.Controls.Rcl
{
    public class CustomButton : Button
    {
        public CustomButton()
        {
            DefaultStyleKey = typeof (CustomButton);
            HotKey = Key.None;
            ShowHotKeyInTitle = true;
        }

        #region . Properties .

        private Key _hotKey;
        public Key HotKey
        {
            get { return _hotKey; }
            set
            {
                _hotKey = value;
                SetContent();
            }
        }

        public Key HotKey2 { get; set; }
        public bool ShowHotKeyInTitle { get; set; }
        public bool IsNotMenuButton { get; set; }

        public bool TransferHotKeyToControls { get; set; }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(CustomButton), new PropertyMetadata(OnTextChanged));

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CustomButton)d).SetContent();
        }

        public string SuffixText
        {
            get { return (string)GetValue(SuffixTextProperty); }
            set { SetValue(SuffixTextProperty, value); }
        }
        public static readonly DependencyProperty SuffixTextProperty = DependencyProperty.Register("SuffixText", typeof(string), typeof(CustomButton));

        #endregion . Properties .

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            PreviewHotKey(e);
            base.OnPreviewKeyDown(e);
        }

        private void SetContent()
        {
            if (ShowHotKeyInTitle && HotKey != Key.None)
                    Content = string.Format("{0} {1}", EnumFormatters.Format(HotKey), Text);
                else
                    Content = Text;
        }

        public bool IsHotKey(Key key)
        {
            return Visibility == Visibility.Visible && IsEnabled && (HotKey == key || HotKey2 == key);
        }

        public bool PreviewHotKey(KeyEventArgs e)
        {
            if (IsHotKey(e.Key))
            {
                Focus();
                OnClick();
                e.Handled = true;
            }
            return e.Handled;
        }

        protected override void OnClick()
        {
            if (!IsNotMenuButton)
            {
                var parent = FindParentWindow();
                if (parent != null)
                {
                    //Ищем лукапы
                    if (VisualTreeHelperExt.FindChildsByType<CustomComboBoxEditRcl>(parent).Any(lookup => lookup.PreviewHotKey(HotKey, HotKey2)))
                        return;

                    if (TransferHotKeyToControls)
                    {
                        //Ищем списки
                        if (VisualTreeHelperExt.FindChildsByType<CustomSelectControl>(parent)
                                .Any(c =>
                                {
                                    if (c.Visibility == Visibility.Visible && c.IsEnabled)
                                        c.Focus();
                                    return c.PreviewHotKey(HotKey, HotKey2);
                                }))
                            return;
                    }
                }
            }
            base.OnClick();
        }

        private Window FindParentWindow()
        {
            var result = VisualTreeHelperExt.GetLogicalParent<Window>(this);
            return result ?? Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
        }
    }
}
