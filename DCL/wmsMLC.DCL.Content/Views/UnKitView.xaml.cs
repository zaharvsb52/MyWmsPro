using DevExpress.Xpf.Grid;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Main.Views.Controls;

namespace wmsMLC.DCL.Content.Views
{
    public partial class UnKitView : DXPanelView, IHelpHandler
    {
        public UnKitView()
        {
            InitializeComponent();
        }

        private void OnSelectedItemChanged(object sender, SelectedItemChangedEventArgs e)
        {
            // т.к. у нас нет возможности управлять FocusedRow - задаем вручную
            var grid = ((CustomGridControl) sender);
            var handles = grid.GetSelectedRowHandles();
            if (handles.Length > 0)
            {
                grid.View.FocusedRowHandle = handles[0];
            }
        }
        
        #region .  IHelpHandler  .

        string IHelpHandler.GetHelpLink()
        {
            return "UnKit";
        }

        string IHelpHandler.GetHelpEntity()
        {
            return "UnKit";
        }

        #endregion
    }
}

