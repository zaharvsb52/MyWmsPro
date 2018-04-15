using System.Diagnostics;
using System.Windows.Navigation;

namespace wmsMLC.DCL.Main.Views
{
    public partial class AboutBoxView : BaseDialogWindow
    {
        public AboutBoxView()
        {
           InitializeComponent();
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(string.Format("mailto:{0}", e.Uri.OriginalString)));
            e.Handled = true;
        }
    }
}