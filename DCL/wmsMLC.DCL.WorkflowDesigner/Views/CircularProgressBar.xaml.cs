using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace wmsMLC.DCL.WorkflowDesigner.Views
{
    /// <summary>
    /// Interaction logic for CircularProgressBar.xaml
    /// </summary>
    public partial class CircularProgressBar : UserControl
    {
        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private readonly Ellipse[] _circle;

        public CircularProgressBar()
        {
            InitializeComponent();
            this.Visibility = System.Windows.Visibility.Hidden;
            this._timer.Interval = TimeSpan.FromMilliseconds(110);
            this._timer.Tick += this.Update;

            _circle = new Ellipse[8];
            _circle[0] = Ellipse1;
            _circle[1] = Ellipse2;
            _circle[2] = Ellipse3;
            _circle[3] = Ellipse4;
            _circle[4] = Ellipse5;
            _circle[5] = Ellipse6;
            _circle[6] = Ellipse7;
            _circle[7] = Ellipse8;
        }

        public static readonly DependencyProperty IsRunningProperty =
            DependencyProperty.Register("IsRunning", typeof(bool), typeof(CircularProgressBar), new PropertyMetadata(OnIsRunningChanged));

        public bool IsRunning
        {
            get { return (bool)GetValue(IsRunningProperty); }
            set { SetValue(IsRunningProperty, value); }
        }

        public void Start()
        {
            this._timer.Start();
        }

        public void Stop()
        {
            this._timer.Stop();
        }

        private static void OnIsRunningChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var bar = sender as CircularProgressBar;
            if (bar == null) return;
            if (!(e.NewValue is bool)) return;
            var newValue = (bool)e.NewValue;

            if (newValue)
            {
                bar.Visibility = System.Windows.Visibility.Visible;
                bar.Start();
            }
            else
            {
                bar.Visibility = System.Windows.Visibility.Hidden;
                bar.Stop();
            }
        }

        private void Update(object sender, EventArgs e)
        {
            Brush ellipse8LastBrush = Ellipse8.Fill;

            for (int index = 7; index > 0; index--)
            {
                _circle[index].Fill = _circle[index - 1].Fill;
            }

            Ellipse1.Fill = ellipse8LastBrush;
        }
    }
}
