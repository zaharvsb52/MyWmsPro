using System;
using System.Collections;
using System.IO;
using System.Windows;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.TreeList;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views.Controls;

namespace wmsMLC.DCL.Main.Views
{
    public partial class CustomParamValueTreeView : IHelpHandler
    {
        private ICustomParamValueTreeViewModel _vm;
        public CustomParamValueTreeView()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _vm = DataContext as ICustomParamValueTreeViewModel;
            if (_vm != null)
            {
                _vm.InitializeMenus();
            }

            SubscribeSource();

            DataContextChanged -= OnDataContextChanged;
        }

        private void SubscribeSource()
        {
            var vm = _vm as IModelHandler;
            if (vm == null)
                return;

            UnSubscribeSource();

            if (_vm != null)
            {
                _vm.NeedChangeFocusRow += OnNeedChangeFocusRow;
            }

            var vme = _vm as IExportData;
            if (vme != null)
                vme.SourceExport += OnSourceExport;
        }

        private void UnSubscribeSource()
        {
            if (_vm != null)
            {
                _vm.NeedChangeFocusRow -= OnNeedChangeFocusRow;
            }
            var vm = _vm as IModelHandler;
            if (vm == null)
                return;

            var vme = _vm as IExportData;
            if (vme != null)
                vme.SourceExport -= OnSourceExport;
        }

        private void OnNeedChangeFocusRow(object sender, EventArgs eventArgs)
        {
            SetFocusedRowHandle();
        }

        private void OnItemsSourceChanged(object sender, ItemsSourceChangedEventArgs e)
        {
            if (e.NewItemsSource == null)
                return;

            SetFocusedRowHandle();

            if (_vm != null)
                _vm.ShowErrorMessage();
        }

        private void SetFocusedRowHandle()
        {
            if (_vm == null)
                return;

            var view = TlCpv.View;
            if (_vm.AutoExpandAllNodes)
                view.ExpandAllNodes();

            if (_vm.SelectedItem == null)
                return;

            var itemsSource = TlCpv.ItemsSource as IList;
            if (itemsSource == null)
                return;

            try
            {
                TlCpv.BeginSelection();
                TlCpv.UnselectAll();

                var ikh = _vm.SelectedItem as CustomParamValue;
                if (ikh == null)
                    return;

                var index = TlCpv.IndexOf(ikh.GetKey());
                if (index < 0)
                    return;

                TlCpv.SelectItem(index);
                view.FocusedRowHandle = index;
            }
            finally
            {
                TlCpv.EndSelection();
            }
        }

        private void OnSourceExport(object sender, EventArgs e)
        {
            var vme = _vm as IExportData;
            if (vme == null)
                return;

            var stream = new MemoryStream();
            TlCpv.SaveLayoutToStream(stream);

            vme.StreamExport = stream;
        }

        private void OnNodeDoubleClick(object sender, RowDoubleClickEventArgs e)
        {
            if (e.HitInfo.RowHandle != TlCpv.View.FocusedNode.RowHandle)
                e.Handled = true;
            else
                if (TlCpv.View.FocusedNode.HasChildren)
                    TlCpv.View.FocusedNode.IsExpanded = !TlCpv.View.FocusedNode.IsExpanded;
        }

        private void TreeListView_OnCustomNodeFilter(object sender, TreeListNodeFilterEventArgs e)
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
        }

        string IHelpHandler.GetHelpLink()
        {
            var dc = _vm as IHelpHandler;
            if (dc == null) 
                return "Tree";
            var link = dc.GetHelpLink();
            return link ?? "Tree";
        }

        string IHelpHandler.GetHelpEntity()
        {
            var dc = _vm as IHelpHandler;
            return dc == null ? string.Empty : dc.GetHelpEntity();
        }

        private bool _inCollapsingOrExpanding;

        private void BarItemCollapseAll_OnItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                _inCollapsingOrExpanding = true;
                TlCpv.View.CollapseAllNodes();
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
                TlCpv.View.ExpandAllNodes();
            }
            finally
            {
                _inCollapsingOrExpanding = false;
            }
        }

        public override bool CanClose()
        {
            var res = base.CanClose();
            if (res)
                UnSubscribeSource();

            return res;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnSubscribeSource();
                DataContextChanged -= OnDataContextChanged;
                TlCpv.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
