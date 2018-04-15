using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using DevExpress.Xpf.Core;
using wmsMLC.DCL.Main.Helpers;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Main.Views.Controls
{
    public class CustomDXDialog : DXDialog, IView, ISaveRestore, IDisposable
    {
        #region .  Constructors  .
        public CustomDXDialog()
        {
            TrueResult = null;

            //HACK: Определяем нажатие Ctrl + F4 /Alt + F4
            var ishooked = false;
            Loaded += delegate
                {
                    if (ishooked) 
                        return;
                    ishooked = true;
                    var source = (HwndSource)PresentationSource.FromDependencyObject(this);
                    if (source != null) 
                        source.AddHook(WindowProc);

                    var button = GetVisualByName(ButtonParts.PART_CloseButton.ToString()) as Button;
                    if (button != null)
                        button.Click += delegate
                        {
                            TrueResult = false;
                        };

                    PreviewKeyDown += (s, e) =>
                    {
                        if (!NoButtons || NoActionOnCancelKey)
                            return;

                        if (e.Key == Key.Escape)
                        {
                            e.Handled = true;
                            Close();
                        }
                    };
                };
        }

        public CustomDXDialog(string title) : base(title)
        {
            TrueResult = null;
        }

        public CustomDXDialog(string title, DialogButtons dialogButtons) : base(title, dialogButtons)
        {
            TrueResult = null;
        }

        public CustomDXDialog(string title, DialogButtons dialogButtons, bool setButtonHandlers)
            : base(title, dialogButtons, setButtonHandlers)
        {
            TrueResult = null;
        }
        #endregion

        protected override void OnSourceInitialized(EventArgs e)
        {
            //HACK:
            //TODO: Сделать по нормальному.
            //var screenWidth = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
            //var screenHeight = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
            //var screenWidth = SystemParameters.PrimaryScreenWidth;
            //var screenHeight = SystemParameters.PrimaryScreenHeight;
            var screenWidth = SystemParameters.FullPrimaryScreenWidth;
            var screenHeight = SystemParameters.FullPrimaryScreenHeight;

            //var width = 0.7d * screenWidth;
            //var height = 0.7d * screenHeight;
            var width = screenWidth;
            var height = screenHeight;

            var sizechanged = false;
            if (Height > height)
            {
                SizeToContent = SizeToContent.Manual;
                Height = height;
                sizechanged = true;
            }

            if (Width > width)
            {
                SizeToContent = SizeToContent.Manual;
                Width = width;
                sizechanged = true;
            }

            //var minwidth = screenWidth*0.1927;
            //if (ActualWidth < minwidth)
            //{
            //    Width = minwidth;
            //    sizechanged = true;
            //}

            if (sizechanged && WindowStartupLocation == WindowStartupLocation.CenterScreen)
            {
                Left = (screenWidth - Width) / 2;
                Top = (screenHeight - Height) / 2;
            }

            // При указании NoButtons кнопки скрываются, но панель, на которой они находятся остается. Нужно ее Collaps-ить
            // Таким странным образом добираемся до Footer панели, т.к. по другому не получается. this.Footer всегда null
            if (NoButtons)
            {
                var footerPanel = YesButton == null ? null : YesButton.Parent as FrameworkElement;
                if (footerPanel != null)
                    footerPanel.Visibility = Visibility.Collapsed;
            }

            base.OnSourceInitialized(e);
        }

        protected override void SetButtonHandlers()
        {
            if (NoButtons)
                return;

            if (YesButton != null)
                YesButton.Click += (sender, args) =>
                {
                    DialogResult = true;
                    TrueResult = true;
                };
            if (NoButton != null)
                NoButton.Click += (sender, args) =>
                {
                    TrueResult = false;
                };
            if (OkButton != null)
                OkButton.Click += (sender, args) =>
                {
                    DialogResult = true;
                    TrueResult = true;
                };
            if (CancelButton != null)
                CancelButton.Click += (sender, args) =>
                {
                    TrueResult = null;
                };

            base.SetButtonHandlers();

            //DialogResult = null;
        }

        //HACK: борьба с двойным закрытием окна
        protected override void OnButtonClick(bool? result, MessageBoxResult messageBoxResult)
        {
        }

        public bool? TrueResult { get; set; }
        public bool NoButtons { get; set; }
        public bool NoActionOnCancelKey { get; set; }

        protected override void SetButtonVisibilities(bool ok, bool cancel, bool yes, bool no, bool apply)
        {
            if (NoButtons)
            {
                ok = cancel = yes = no = apply = false;
            }
            base.SetButtonVisibilities(ok, cancel, yes, no, apply);
        }

        /// <summary>
        /// Определяем закрытие окна по Ctrl + F4
        /// http://bytes.com/topic/c-sharp/answers/812435-wpf-question-no-closereason
        /// </summary>
        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x112 && (Loword((int)wParam) & 0xfff0) == 0xf060) //Ctrl + F4 /Alt + F4
            {
                TrueResult = false;
            }

            return IntPtr.Zero;
        }

        private static int Loword(int n)
        {
            return (n & 0xffff);
        }

        #region .  Finalize & Dispose  .
        /// <summary> Признак того, что освобождение ресурсов уже произошло </summary>
        public bool IsDisposed { get; private set; }

        ~CustomDXDialog()
        {
            if (IsDisposed)
                return;

            Dispose(false);
            IsDisposed = true;
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            Dispose(true);
            GC.SuppressFinalize(this);
            IsDisposed = true;
        }

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        /// <param name="disposing">False - если требуется освободить только UnManaged ресурсы, True - если еще и Managed</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                var dispContent = this.Content as IDisposable;
                if (dispContent != null)
                    dispContent.Dispose();
                this.Content = null;
            }
        }
        #endregion
    }
}