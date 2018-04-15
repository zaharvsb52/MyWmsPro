using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.RCL.Main.ViewModels;

namespace wmsMLC.RCL.Main.Views
{
    public partial class AuthenticationView
    {
        public AuthenticationView()
        {
            InitializeComponent();
            DataContextChanged += delegate
                {
                    var cr = DataContext as AuthenticationViewModel;
                    if (cr != null)
                    {
                        cr.CloseRequest += (s, e) =>
                        {
                            Hide();
                            Close();
                        };
                        cr.DataError += (s, e) => Psw.Focus();

                        if (!string.IsNullOrEmpty(cr.Login))
                            Psw.BackgroundFocus();
                    }
                };
#if DEBUG
            Topmost = false;
#endif
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            Focus();
            Activate();
        }
    }
}
