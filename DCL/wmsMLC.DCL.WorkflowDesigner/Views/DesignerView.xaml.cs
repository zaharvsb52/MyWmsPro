using System.Windows.Controls;

namespace wmsMLC.DCL.WorkflowDesigner.Views
{
    /// <summary>
    /// Interaction logic for DesignerView.xaml
    /// </summary>
    public partial class DesignerView : UserControl
    {
        public DesignerView()
        {
            InitializeComponent();
        }

        public DesignerView(IDesignerViewModel designerViewModel)
        {
            InitializeComponent();
            this.DataContext = designerViewModel;
        }
    }
}
