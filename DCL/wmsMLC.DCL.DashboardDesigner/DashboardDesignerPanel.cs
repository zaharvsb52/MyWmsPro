using System.Windows.Forms;

namespace wmsMLC.DCL.DashboardDesigner
{
    public partial class DashboardDesignerPanel : UserControl
    {
        public DevExpress.DashboardWin.DashboardDesigner Designer
        {
            get { return dashboardDesigner; }
        }

        public DashboardDesignerPanel()
        {
            InitializeComponent();
        }
    }
}
