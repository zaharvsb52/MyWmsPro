using System.Windows.Input;
using DevExpress.Xpf.Core;
using wmsMLC.DCL.Main.Helpers;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Main.Views
{
    public class BaseDialogWindow : DXWindow, IView, ISaveRestore
    {
        public BaseDialogWindow()
        {
            TextOptionsHelper.GetTextOptions(this);
        }

        public bool NotCloseOnEscapeKey { get; set; }
        
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (!NotCloseOnEscapeKey && e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
                return;
            }
            base.OnPreviewKeyDown(e);
        }
    }
}