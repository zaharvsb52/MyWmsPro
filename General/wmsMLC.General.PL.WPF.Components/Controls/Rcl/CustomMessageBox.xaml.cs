using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace wmsMLC.General.PL.WPF.Components.Controls.Rcl
{
    public partial class CustomMessageBox
    {
        #region .  Fields  .
        private string _caption;
        private string _text;
        private MessageBoxImage _image;
        private MessageBoxButton _button;
        #endregion

        protected CustomMessageBox()
        {
            MessageBoxResult = MessageBoxResult.None;

            InitializeComponent();
#if DEBUG
            Topmost = false;
#endif
            //DataContext = this;
        }

        #region .  Properties  .
        public string Caption
        {
            get { return _caption; }
            private set
            {
                if (_caption == value)
                    return;
                _caption = value;
                tCaption.Text = _caption;
            }
        }

        public string Text
        {
            get { return _text; }
            private set
            {
                if (_text == value)
                    return;
                _text = value;
                tText.EditValue = _text;
            }
        }

        public MessageBoxImage Image
        {
            get { return _image; }
            private set
            {
                if (_image == value)
                    return;
                _image = value;
                image.Source = InitImageSource(value);
            }
        }

        public MessageBoxButton Button
        {
            get { return _button; }
            private set
            {
                _button = value;
                okGrid.Visibility = Visibility.Collapsed;
                okCancelGrid.Visibility = Visibility.Collapsed;
                yesNoGrid.Visibility = Visibility.Collapsed;
                yesNoCancelGrid.Visibility = Visibility.Collapsed;

                switch (_button)
                {
                    case MessageBoxButton.OKCancel:
                        okCancelGrid.Visibility = Visibility.Visible;
                        break;
                    case MessageBoxButton.YesNo:
                        yesNoGrid.Visibility = Visibility.Visible;
                        break;
                    case MessageBoxButton.YesNoCancel:
                        yesNoCancelGrid.Visibility = Visibility.Visible;
                        break;
                    default:
                        okGrid.Visibility = Visibility.Visible;
                        break;
                }
            }
        }

        public MessageBoxResult MessageBoxResult { get; private set; }

        public MessageBoxResult DefaultResult { get; private set; }
        #endregion

        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button,
            MessageBoxImage icon, MessageBoxResult defaultResult, double? fontSize)
        {
            var window = new CustomMessageBox
            {
                Title = caption,
                Caption = caption,
                Text = messageBoxText,
                Button = button,
                Image = icon,
                DefaultResult = defaultResult
            };

            if (fontSize.HasValue)
            {
                window.FontSize = fontSize.Value;
                window.RefreshFontSize();
            }

            BeepByImage(icon);
            window.ShowDialog();
            return window.MessageBoxResult;
        }

        private void RefreshFontSize()
        {
            tCaption.FontSize = FontSize;
            tText.FontSize = FontSize;
        }

        private static void BeepByImage(MessageBoxImage icon)
        {
            switch (icon)
            {
                case MessageBoxImage.Exclamation:
                    System.Media.SystemSounds.Exclamation.Play();
                    break;
                case MessageBoxImage.Asterisk:
                    System.Media.SystemSounds.Asterisk.Play();
                    break;
                case MessageBoxImage.Hand:
                    System.Media.SystemSounds.Hand.Play();
                    break;
                case MessageBoxImage.Question:
                    System.Media.SystemSounds.Question.Play();
                    break;
            }
        }

        private static ImageSource InitImageSource(MessageBoxImage icon)
        {
            switch (icon)
            {
                case MessageBoxImage.Question:
                    return Properties.Resources.Question_48x48.GetBitmapImage();
                case MessageBoxImage.Exclamation:
                    return Properties.Resources.Warning_48x48.GetBitmapImage();
                case MessageBoxImage.Asterisk:
                    return Properties.Resources.Information_48x48.GetBitmapImage();
                case MessageBoxImage.None:
                    return null;
                case MessageBoxImage.Hand:
                    return Properties.Resources.Error_48x48.GetBitmapImage();

                default:
                    throw new DeveloperException("Unsupported MessageBoxImage type " + icon);
            }
        }

        protected override void OnWindowClose(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    switch (Button)
                    {
                        //case MessageBoxButton.OK:
                        //    // выставляем значение только если явно указали default
                        //    if (DefaultResult == MessageBoxResult.OK)
                        //        MessageBoxResult = MessageBoxResult.OK;
                        //    else
                        //        System.Media.SystemSounds.Exclamation.Play();
                        //    break;
                        case MessageBoxButton.OKCancel:
                            MessageBoxResult = MessageBoxResult.OK;
                            break;
                        case MessageBoxButton.YesNo:
                        case MessageBoxButton.YesNoCancel:
                            MessageBoxResult = MessageBoxResult.Yes;
                            break;
                    }
                    break;
                case Key.Escape:
                    switch (Button)
                    {
                        case MessageBoxButton.OK:
                        case MessageBoxButton.OKCancel:
                            MessageBoxResult = MessageBoxResult.Cancel;
                            break;
                        case MessageBoxButton.YesNo:
                        case MessageBoxButton.YesNoCancel:
                            MessageBoxResult = MessageBoxResult.No;
                            break;
                    }
                    break;
            }

            e.Handled = MessageBoxResult != MessageBoxResult.None;
            if (e.Handled)
                Close();
        }

        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult = MessageBoxResult.OK;
            Close();
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult = MessageBoxResult.Cancel;
            Close();
        }

        private void OnYesButtonClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult = MessageBoxResult.Yes;
            Close();
        }

        private void OnNoButtonClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult = MessageBoxResult.No;
            Close();
        }
    }
}