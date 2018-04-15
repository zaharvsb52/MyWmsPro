using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace wmsMLC.General.PL.WPF.Components.Controls.Rcl
{
    public class CustomWaitIndicator : ContentControl
    {
        public event EventHandler IsBusyChanged;

        private readonly DispatcherTimer _displayAfterTimer;

        public CustomWaitIndicator()
        {
            DefaultStyleKey = typeof (CustomWaitIndicator);
            _displayAfterTimer = new DispatcherTimer();
            _displayAfterTimer.Tick += delegate
                {
                    _displayAfterTimer.Stop();
                    IsBusyIndicationVisible = true;
                    DoEvents();
                };
        }

        #region . Properties .
        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }
        public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register("IsBusy", typeof(bool), typeof(CustomWaitIndicator), new PropertyMetadata(OnIsBusyPropertyChanged));

        private static void OnIsBusyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CustomWaitIndicator)d).OnIsBusyPropertyChanged();
        }

        private void OnIsBusyPropertyChanged()
        {
            SetPropertiesAccordingIsBusy();
            OnIsBusyChanged();
        }

        public string BusyContent
        {
            get { return (string)GetValue(BusyContentProperty); }
            set { SetValue(BusyContentProperty, value); }
        }
        public static readonly DependencyProperty BusyContentProperty = DependencyProperty.Register("BusyContent", typeof(string), typeof(CustomWaitIndicator));

        public TimeSpan DisplayAfter
        {
            get { return (TimeSpan)GetValue(DisplayAfterProperty); }
            set { SetValue(DisplayAfterProperty, value); }
        }
        public static readonly DependencyProperty DisplayAfterProperty = DependencyProperty.Register("DisplayAfter", typeof(TimeSpan), typeof(CustomWaitIndicator), new PropertyMetadata(TimeSpan.FromSeconds(0.1)));

        public bool IsBusyIndicationVisible
        {
            get { return (bool) GetValue(IsBusyIndicationVisibleProperty); }
            private set { SetValue(IsBusyIndicationVisibleProperty, value); }
        }
        public static readonly DependencyProperty IsBusyIndicationVisibleProperty = DependencyProperty.Register("IsBusyIndicationVisible", typeof(bool), typeof(CustomWaitIndicator));
        #endregion . Properties .

        #region . Methods .
        private void OnIsBusyChanged()
        {
            var handler = IsBusyChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void SetPropertiesAccordingIsBusy()
        {
            if (IsBusy)
            {
                if (DisplayAfter == TimeSpan.Zero)
                {
                    IsBusyIndicationVisible = true;
                }
                else
                {
                    _displayAfterTimer.Interval = DisplayAfter;
                    _displayAfterTimer.Start();
                }
            }
            else
            {
                _displayAfterTimer.Stop();
                IsBusyIndicationVisible = false;
            }
            DoEvents();
        }

        private void DoEvents()
        {
            //Исключаем ошибку: System.InvalidOperationException: Не удается выполнить данную операцию, пока работа диспетчера приостановлена.
            if(Dispatcher.Thread.ThreadState == System.Threading.ThreadState.Suspended)
                return;
 
            var frame = new DispatcherFrame();
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        private object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;
            return null;
        }
        #endregion . Methods .
    }
}
