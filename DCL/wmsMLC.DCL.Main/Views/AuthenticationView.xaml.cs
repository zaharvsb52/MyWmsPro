using System;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using wmsMLC.General.PL.WPF;

namespace wmsMLC.DCL.Main.Views
{
    /// <summary>
    /// NOTE: не используем здесь DXWindow, т.к. оно не знает как работать с WindowStyle
    /// puflink: http://www.devexpress.com/Support/Center/p/B149221.aspx
    /// </summary>
    public partial class AuthenticationView : Window
    {
        public AuthenticationView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnLoaded;

            //Контролируем подительское окно. Если не задано, то Topmost = true
            if (Owner == null && new WindowInteropHelper(this).Owner == IntPtr.Zero)
                Topmost = true;

            Activate();
            Focus();
            Keyboard.Focus(passwordEdit);

            image.Source = Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Warning.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            ChangeVisiviltyCapsLosk();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var cr = DataContext as ICloseRequest;
            if (cr != null)
                cr.CloseRequest += OnCloseRequest;
        }

        private void OnCloseRequest(object sender, EventArgs eventArgs)
        {
            Close();

            var cr = DataContext as ICloseRequest;
            if (cr != null)
                cr.CloseRequest -= OnCloseRequest;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AuthenticationView_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.CapsLock)
                return;

            ChangeVisiviltyCapsLosk();
        }

        private void ChangeVisiviltyCapsLosk()
        {
            capsLockOn.Visibility = Keyboard.IsKeyToggled(Key.CapsLock) ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
