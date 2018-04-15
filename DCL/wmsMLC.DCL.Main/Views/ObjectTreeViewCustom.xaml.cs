using System;
using System.Collections;
using System.IO;
using DevExpress.Xpf.Grid;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels;

namespace wmsMLC.DCL.Main.Views
{
    /// <summary>
    /// Interaction logic for ObjectTreeViewCustom.xaml
    /// </summary>
    public partial class ObjectTreeViewCustom : DXPanelView, IHelpHandler
    {
        public static ITreeViewModelM2M CustomDataContext;

        public ObjectTreeViewCustom()
        {
            InitializeComponent();
            DataContextChanged += ObjectTreeViewCustom_DataContextChanged;
        }

        void ObjectTreeViewCustom_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var vm = DataContext as IListViewModel;
            if (vm != null)
            {
                vm.InitializeMenus();
            }
            CustomDataContext = DataContext as ITreeViewModelM2M;

            var vme = DataContext as IExportData;
            if (vme == null) 
                return;

            vme.SourceExport += OnSourceExport;
            vme.SourceExport -= OnSourceExport;
        }

        private void OnSourceExport(object sender, EventArgs eventArgs)
        {
            var vme = DataContext as IExportData;
            if (vme == null)
                return;

            var stream = new MemoryStream();
            customTreeListControl.SaveLayoutToStream(stream);

            vme.StreamExport = stream;
        }

        private void NodeDoubleClick(object sender, RowDoubleClickEventArgs rowDoubleClickEventArgs)
        {
           if (!customTreeListControl.View.FocusedNode.HasChildren)
              return;
           customTreeListControl.View.FocusedNode.IsExpanded = !customTreeListControl.View.FocusedNode.IsExpanded;
        }

        string IHelpHandler.GetHelpLink()
        {
            var dc = DataContext as IHelpHandler;
            if (dc != null)
            {
                var link = dc.GetHelpLink();
                if (link != null)
                    return link;
            }
            return "Tree";
        }

        string IHelpHandler.GetHelpEntity()
        {
            var dc = DataContext as IHelpHandler;
            return dc == null ? string.Empty : dc.GetHelpEntity();
        }

        #region .  IDisposable  .
        protected override void Dispose(bool disposing)
        {
            DataContextChanged -= ObjectTreeViewCustom_DataContextChanged;
            customTreeListControl.Dispose();
            base.Dispose(disposing);
        }
        #endregion


    }

    public class ChildSelector : IChildNodesSelector
    {
        IEnumerable IChildNodesSelector.SelectChildren(object item)
        {
            var ret = ObjectTreeViewCustom.CustomDataContext;
            return ret != null ? ret.GetChild(item) : null;
        }
    }
}
