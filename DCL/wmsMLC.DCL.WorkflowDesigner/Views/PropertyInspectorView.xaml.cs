using System.Windows.Controls;

namespace wmsMLC.DCL.WorkflowDesigner.Views
{
    /// <summary>
    /// Interaction logic for PropertyInspectorView.xaml
    /// </summary>
    public partial class PropertyInspectorView : UserControl
    {
        public PropertyInspectorView()
        {
            InitializeComponent();
        }

        public PropertyInspectorView(IDesignerViewModel designerViewModel)
        {
            InitializeComponent();
            this.DataContext = designerViewModel;
        }
    }
}
