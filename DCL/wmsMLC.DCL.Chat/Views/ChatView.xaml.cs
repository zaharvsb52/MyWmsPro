using System;
using System.Linq;
using System.Windows;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.TreeList;
using wmsMLC.DCL.Chat.ViewModels;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Main.Views.Controls;

namespace wmsMLC.DCL.Chat.Views
{
    /// <summary>
    /// Interaction logic for ChatView.xaml
    /// </summary>
    public partial class ChatView : DXPanelView, IHelpHandler
    {
        public ChatView()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
            ChatTabControl.TabHidden += ChatTabControl_TabHidden;
            ChatTabControl.TabHiding += ChatTabControl_TabHiding;
        }

        void ChatTabControl_TabHiding(object sender, DevExpress.Xpf.Core.TabControlTabHidingEventArgs e)
        {
            var model = DataContext as IChatViewModelInternal;
            if (model != null && model.IsRoom(e.TabIndex))
                e.Cancel = true;
        }

        void ChatTabControl_TabHidden(object sender, DevExpress.Xpf.Core.TabControlTabHiddenEventArgs e)
        {
            var model = DataContext as IChatViewModelInternal;
            if(model != null)
                model.CloseTabIndex(e.TabIndex);
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            DataContextChanged -= OnDataContextChanged;
        }


        //private void OnNodeDoubleClick(object sender, RowDoubleClickEventArgs rowDoubleClickEventArgs)
        //{
        //    if (rowDoubleClickEventArgs.HitInfo.RowHandle != customTreeListControl.View.FocusedNode.RowHandle)
        //        rowDoubleClickEventArgs.Handled = true;
        //    else
        //        if (customTreeListControl.View.FocusedNode.HasChildren)
        //            customTreeListControl.View.FocusedNode.IsExpanded = !customTreeListControl.View.FocusedNode.IsExpanded;
        //}

        //private bool NeedShow(TreeListNode node)
        //{
        //    if (string.IsNullOrEmpty(customTreeListControl.View.SearchString))
        //        return true;

        //    if (node.Nodes.Any(NeedShow))
        //        return true;

        //    foreach (var col in customTreeListControl.View.VisibleColumns)
        //    {
        //        var str = customTreeListControl.View.GetNodeValue(node, col).ToString();
        //        if (str.IndexOf(customTreeListControl.View.SearchString, StringComparison.InvariantCultureIgnoreCase) >= 0)
        //            return true;
        //    }

        //    return false;
        //}

        //private void TreeListView_OnCustomNodeFilter(object sender, TreeListNodeFilterEventArgs e)
        //{
        //    if (_inCollapsingOrExpanding)
        //        return;

        //    var tree = sender as CustomTreeListView;
        //    if (tree == null || tree.AutoExpandAllNodes)
        //        return;

        //    if (!string.IsNullOrEmpty(tree.SearchString))
        //        e.Node.IsExpanded = true;
        //    else
        //    {
        //        foreach (var node in tree.Nodes)
        //        {
        //            node.IsExpanded = false;
        //        }
        //    }

        //    if (NeedShow(e.Node))
        //    {
        //        e.Visible = true;
        //        e.Handled = true;
        //    }
        //    else
        //    {
        //        e.Visible = false;
        //        e.Handled = true;
        //    }
        //}

        string IHelpHandler.GetHelpLink()
        {
            var dc = DataContext as IHelpHandler;
            if (dc == null)
                return "Chat";
            var link = dc.GetHelpLink();
            return link ?? "Chat";
        }

        string IHelpHandler.GetHelpEntity()
        {
            var dc = DataContext as IHelpHandler;
            return dc == null ? string.Empty : dc.GetHelpEntity();
        }

        //private bool _inCollapsingOrExpanding;

        //private void BarItemCollapseAll_OnItemClick(object sender, ItemClickEventArgs e)
        //{
        //    try
        //    {
        //        _inCollapsingOrExpanding = true;
        //        customTreeListControl.View.CollapseAllNodes();
        //    }
        //    finally
        //    {
        //        _inCollapsingOrExpanding = false;
        //    }
        //}

        //private void BarItemExpandAll_OnItemClick(object sender, ItemClickEventArgs e)
        //{
        //    try
        //    {
        //        _inCollapsingOrExpanding = true;
        //        customTreeListControl.View.ExpandAllNodes();
        //    }
        //    finally
        //    {
        //        _inCollapsingOrExpanding = false;
        //    }
        //}

        protected override void Dispose(bool disposing)
        {
            ChatTabControl.TabHidden -= ChatTabControl_TabHidden;
            DataContextChanged -= OnDataContextChanged;
            //customTreeListControl.Dispose();
            base.Dispose(disposing);
        }
    }
}
