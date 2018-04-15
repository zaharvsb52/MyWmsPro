using System.Windows.Controls;

namespace wmsMLC.DCL.WorkflowDesigner.Views
{
    /// <summary>
    /// Interaction logic for XamlView.xaml
    /// </summary>
    public partial class XamlView : UserControl
    {
        public XamlView()
        {
            InitializeComponent();
        }

        public XamlView(IXamlViewModel dataContext)
        {
            InitializeComponent();
            this.DataContext = dataContext;
        }
    }
}
