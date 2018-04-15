using System.Linq;
using wmsMLC.General.PL.WPF.Components.Controls.Rcl;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.RCL.Main.Views
{
    public partial class Shell : IShell
    {
        public Shell()
        {
            InitializeComponent();
#if DEBUG
            ResizeMode = System.Windows.ResizeMode.CanResize;
            WindowState = System.Windows.WindowState.Normal;
            WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
#endif
        }

        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                foreach (var tile in VisualTreeHelperExt.FindChildsByType(this, typeof(CustomTile)).OfType<CustomTile>())
                {
                    tile.PreviewHotKey(e);
                }
            }
            finally
            {
                base.OnPreviewKeyDown(e);
            }
        }
    }
}
