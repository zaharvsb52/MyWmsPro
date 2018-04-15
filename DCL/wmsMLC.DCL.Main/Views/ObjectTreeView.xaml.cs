using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.TreeList;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views.Controls;

namespace wmsMLC.DCL.Main.Views
{
    public partial class ObjectTreeView : DXPanelView, IHelpHandler
    {
        public ObjectTreeView()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var vm = DataContext as IListViewModel;
            if (vm != null)
            {
                vm.InitializeMenus();
            }
            var vme = DataContext as IExportData;
            if (vme != null)
            {
                vme.SourceExport -= OnSourceExport;
                vme.SourceExport += OnSourceExport;
            }

            DataContextChanged -= OnDataContextChanged;
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

        private void OnNodeDoubleClick(object sender, RowDoubleClickEventArgs rowDoubleClickEventArgs)
        {
            if (rowDoubleClickEventArgs.HitInfo.RowHandle != customTreeListControl.View.FocusedNode.RowHandle)
                rowDoubleClickEventArgs.Handled = true;
            else
                FocusedNodeExpand();
        }

        private void FocusedNodeExpand()
        {
            if (customTreeListControl.View.FocusedNode.HasChildren)
                customTreeListControl.View.FocusedNode.IsExpanded = !customTreeListControl.View.FocusedNode.IsExpanded;
        }

        private bool NeedShow(TreeListNode node)
        {
            if (string.IsNullOrEmpty(customTreeListControl.View.SearchString))
                return true;

            if (node.Nodes.Any(NeedShow))
                return true;

            foreach (var col in customTreeListControl.View.VisibleColumns)
            {
                var obj = customTreeListControl.View.GetNodeValue(node, col);
                if (obj == null)
                    continue;
                var str = obj.ToString();
                if (str.IndexOf(customTreeListControl.View.SearchString, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        private void OnCustomNodeFilter(object sender, TreeListNodeFilterEventArgs e)
        {
            if (_inCollapsingOrExpanding)
                return;

            var tree = sender as CustomTreeListView;
            if (tree == null || tree.AutoExpandAllNodes) 
                return;

            if (!string.IsNullOrEmpty(tree.SearchString))
                e.Node.IsExpanded = true;
            else
            {
                foreach (var node in tree.Nodes)
                {
                    node.IsExpanded = false;
                }
            }

            if (NeedShow(e.Node))
            {
                e.Visible = true;
                e.Handled = true;
            }
            else
            {
                e.Visible = false;
                e.Handled = true;
            }
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // если мы сейчас в режиме редактирования фильтра и нажали Enter, то нужно получить данные
            //TODO: Добавить проверку на нахождение в строке автопоиска (вот такая не работает - View.FocusedRowHandle == DevExpress.Data.CurrencyDataController.FilterRow)
            if (e.Key == Key.Enter && customTreeListControl.View.FocusedRowHandle < 0)
            {
                var listModel = DataContext as IListViewModel;
                if (listModel != null)
                {
                    // заставляем вычитать данные
                    listModel.ApplyFilter();
                    e.Handled = true;
                }
            }
            else
            {
                var objectTreeViewModel = DataContext as IObjectTreeViewModel;
                if (objectTreeViewModel != null)
                {
                    var editKey = objectTreeViewModel.GetEditKey();
                    if (editKey != null && editKey.Matches(null, e))
                    {
                        e.Handled = true;
                        FocusedNodeExpand();
                        if (objectTreeViewModel.EditCommand != null && objectTreeViewModel.EditCommand.CanExecute(null))
                            objectTreeViewModel.EditCommand.Execute(null);
                    }
                }
            }
        }

        string IHelpHandler.GetHelpLink()
        {
            var dc = DataContext as IHelpHandler;
            if (dc == null) 
                return "Tree";
            var link = dc.GetHelpLink();
            return link ?? "Tree";
        }

        string IHelpHandler.GetHelpEntity()
        {
            var dc = DataContext as IHelpHandler;
            return dc == null ? string.Empty : dc.GetHelpEntity();
        }

        private bool _inCollapsingOrExpanding;

        private void BarItemCollapseAll_OnItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                _inCollapsingOrExpanding = true;
                customTreeListControl.View.CollapseAllNodes();
            }
            finally
            {
                _inCollapsingOrExpanding = false;
            }
        }

        private void BarItemExpandAll_OnItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                _inCollapsingOrExpanding = true;
                customTreeListControl.View.ExpandAllNodes();
            }
            finally
            {
                _inCollapsingOrExpanding = false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            DataContextChanged -= OnDataContextChanged;
            customTreeListControl.Dispose();
            base.Dispose(disposing);
        }
    }
}
