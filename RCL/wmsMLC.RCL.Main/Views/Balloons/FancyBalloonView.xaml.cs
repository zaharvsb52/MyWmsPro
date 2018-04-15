using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace wmsMLC.RCL.Main.Views.Balloons
{
    /// <summary>
    /// Interaction logic for FancyBalloonView.xaml
    /// </summary>
    public partial class FancyBalloonView : UserControl
    {
        public static readonly DependencyProperty BalloonTextProperty =
        DependencyProperty.Register("BalloonText",
                                    typeof(string),
                                    typeof(FancyBalloonView));

        public static readonly DependencyProperty BalloonTitleProperty =
        DependencyProperty.Register("BalloonTitle",
                                    typeof(string),
                                    typeof(FancyBalloonView));

        public static readonly DependencyProperty CurrentTimeProperty =
        DependencyProperty.Register("CurrentTime",
                                    typeof(string),
                                    typeof(FancyBalloonView));

        /// <summary>
        /// A property wrapper for the <see cref="BalloonTextProperty"/>
        /// dependency property:<br/>
        /// Description
        /// </summary>
        public string BalloonText
        {
            get { return (string)GetValue(BalloonTextProperty); }
            set
            {
                SetValue(BalloonTextProperty, value);
            }
        }

        /// <summary>
        /// A property wrapper for the <see cref="BalloonTitleProperty"/>
        /// dependency property:<br/>
        /// Description
        /// </summary>
        public string BalloonTitle
        {
            get { return (string)GetValue(BalloonTitleProperty); }
            set { SetValue(BalloonTitleProperty, value); }
        }

        public string CurrentTime
        {
            get
            {
                return (string)GetValue(CurrentTimeProperty);                
            }
            set { SetValue(CurrentTimeProperty, value); }
        }

        private bool isClosing = false;

        public FancyBalloonView()
        {
            InitializeComponent();
            SetBinding(BalloonTextProperty, BalloonTextProperty.Name);
            SetBinding(BalloonTitleProperty, BalloonTitleProperty.Name);
            SetBinding(CurrentTimeProperty, CurrentTimeProperty.Name);            
        }

        private void imgClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //the tray icon assigned this attached property to simplify access            
        }

        private void OnFadeOutCompleted(object sender, EventArgs e)
        {
            //var pp = (Popup)Parent;
            //pp.IsOpen = false;
        }

        private void grid_MouseEnter(object sender, MouseEventArgs e)
        {
            //if we're already running the fade-out animation, do not interrupt anymore
            //(makes things too complicated for the sample)
            if (isClosing) return;

            //the tray icon assigned this attached property to simplify access            
        }

        private void OnBalloonClosing(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            isClosing = true;
        }
    }
}
