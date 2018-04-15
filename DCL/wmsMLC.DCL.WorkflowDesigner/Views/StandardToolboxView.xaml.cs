using System.Windows.Controls;

namespace wmsMLC.DCL.WorkflowDesigner.Views
{
    /// <summary>
    /// Interaction logic for StandardToolboxView.xaml
    /// </summary>
    public partial class StandardToolboxView : UserControl
    {
        public StandardToolboxView()
        {
            InitializeComponent();
        }

        public StandardToolboxView(IToolboxViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
