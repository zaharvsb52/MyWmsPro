using System.Windows.Input;
using wmsMLC.General.PL.WPF.Components.Helpers;

namespace wmsMLC.General.PL.WPF.Components.Views
{
    public partial class CustomComboBoxEditRclContent
    {
        public CustomComboBoxEditRclContent()
        {
            InitializeComponent();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            KeyHelper.ViewPreviewKeyDown(this, e);
            base.OnPreviewKeyDown(e);
        }
    }
}
