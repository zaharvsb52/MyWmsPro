using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using DevExpress.Utils;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.DragDrop;
using DevExpress.Xpf.Grid.TreeList;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Main.Views.Controls;
using wmsMLC.DCL.Packing.ViewModels;

namespace wmsMLC.DCL.Packing.Views
{
    public partial class PackingView : DXPanelView, IHelpHandler
    {
        private const string Packing = "Packing";
        private const string Packed = "Packed";
        private const string TE = "TE";
        private string _dragSource;

        public PackingView()
        {
            InitializeComponent();
        }

        private void ProductOnKeyDown(object sender, KeyEventArgs e)
        {
            var control = sender as ButtonEdit;
            if (control == null)
                return;

            if (e.Key == Key.Enter)
            {
                var expression = control.GetBindingExpression(TextEditBase.TextProperty);
                if (expression != null)
                    expression.UpdateSource();
            }
        }

        private void OnPackingProductSearchButtonClick(object sender, RoutedEventArgs e)
        {
            var expression = bProduct.GetBindingExpression(TextEditBase.TextProperty);
            if (expression != null)
                expression.UpdateSource();
        }

        private void GridPackedDragDropManager_OnDrop(object sender, GridDropEventArgs e)
        {
            e.Handled = true;

            var vm = DataContext as IPackingViewModel;
            if (vm == null)
                return;

            switch (_dragSource)
            {
                case Packing: if(vm.CanPack()) vm.Pack(); break;
                default: e.Handled = false; break;
            }
        }

        private void TreeListTEDragDropManager_OnDrop(object sender, TreeListDropEventArgs e)
        {
            e.Handled = true;

            var vm = DataContext as IPackingViewModel;
            if (vm == null)
                return;
            switch (_dragSource)
            {
                case Packed: if(vm.CanMove()) vm.MoveTo(); break;
            }
        }

        private void GridPackingDragDropManager_OnDrop(object sender, GridDropEventArgs e)
        {
            e.Handled = true;

            var vm = DataContext as IPackingViewModel;
            if (vm == null)
                return;

            switch (_dragSource)
            {
                case Packed: if(vm.CanReturnOnSourceTE()) vm.ReturnOnSourceTE(); break;
                default: e.Handled = false; break;
            }
        }

        private void GridPackingDragDropManager_OnStartDrag(object sender, GridStartDragEventArgs e)
        {
            var vm = DataContext as IPackingViewModel;
            if (vm == null)
                return;

            _dragSource = Packing;
        }

        private void GridPackedDragDropManager_OnStartDrag(object sender, GridStartDragEventArgs e)
        {
            var vm = DataContext as IPackingViewModel;
            if (vm == null)
                return;

            _dragSource = Packed;
        }

        private void TreeListTEDragDropManager_OnStartDrag(object sender, TreeListStartDragEventArgs e)
        {
            var vm = DataContext as IPackingViewModel;
            if (vm == null)
                return;

            _dragSource = TE;
        }

        private void TreeListTEDragDropManager_OnDragOver(object sender, TreeListDragOverEventArgs e)
        {
            var manager = sender as GridDragDropManager;
            if (manager == null)
                return;

            var vm = DataContext as IPackingViewModel;
            if (vm == null)
                return;

            switch (_dragSource)
            {
                case Packed:
                    {
                        if (!vm.CanMove())
                        {
                            if (manager.AllowDrop) manager.AllowDrop = false;
                        }
                        else if (!manager.AllowDrop) manager.AllowDrop = true;
                        break;
                    }
                default: if (manager.AllowDrop) manager.AllowDrop = false; break;
            }
        }

        private void GridPackingDragDropManager_OnDragOver(object sender, GridDragOverEventArgs e)
        {
            var manager = sender as GridDragDropManager;
            if (manager == null)
                return;
          
            switch (_dragSource)
            {
                case Packed: if (!manager.AllowDrop) manager.AllowDrop = true; break;
                default: if (manager.AllowDrop) manager.AllowDrop = false; break;
            }
        }


        private void GridPackedDragDropManager_OnDragOver(object sender, GridDragOverEventArgs e)
        {
            var manager = sender as GridDragDropManager;
            if (manager == null)
                return;

            switch (_dragSource)
            {
                case Packing: if (!manager.AllowDrop) manager.AllowDrop = true; break;
                default: if (manager.AllowDrop) manager.AllowDrop = false; break;
            }
        }
      
        private void OnSelectedItemChanged(object sender, SelectedItemChangedEventArgs e)
        {
            // т.к. у нас нет возможности управлять FocusedRow - задаем вручную
            var grid = ((CustomTreeListControl)sender);
            var handles = grid.GetSelectedRowHandles();
            if (handles.Length > 0)
            {
                grid.View.FocusedRowHandle = handles[0];
                grid.HaveChild = grid.GetSelectedNodes()[0].Nodes != null && grid.GetSelectedNodes()[0].Nodes.Count > 0;
            }
        }

        #region .  IHelpHandler  .
        string IHelpHandler.GetHelpLink()
        {
            return "Packing";
        }

        string IHelpHandler.GetHelpEntity()
        {
            return "Packing";
        }
        #endregion

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var grid = sender as GridControl;
            if (grid == null)
            {
                var treeList = sender as TreeListControl;
                if (treeList == null)
                    return;

                var treeListView = treeList.View;
                if (treeListView == null || treeListView.FocusedRowHandle == TreeListControl.AutoFilterRowHandle)
                    return;
            }
            else
            {
                var gridview = grid.View;
                if (gridview == null || gridview.FocusedRowHandle == GridControl.AutoFilterRowHandle)
                    return;
            }

            bProduct.Focusable = true;
            Keyboard.Focus(bProduct);
        }

        private bool _inCollapsingOrExpanding;

        private void BarItemCollapseAll_OnItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                _inCollapsingOrExpanding = true;
                gTE.View.CollapseAllNodes();
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
                gTE.View.ExpandAllNodes();
            }
            finally
            {
                _inCollapsingOrExpanding = false;
            }
        }
      
        private void OnCustomNodeFilter(object sender, TreeListNodeFilterEventArgs e)
        {
            if (_inCollapsingOrExpanding)
                return;

            foreach (var item in e.Node.Nodes.Where(item => item.IsFiltered == false))
            {
                e.Visible = true;
                e.Handled = true;
            }
        }
    }

    public class IsActiveConverter : MarkupExtension, IMultiValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var te = values[0] as TE;
            if (te == null)
                return false;

            var rd = values[1] as RowData;
            if (rd == null)
                return false;

            if (rd.View.DataControl.CurrentItem != rd.Row)
                return false;

            var rowTe = rd.Row as TE;
            if (rowTe == null || rowTe.TECode != te.TECode)
                return false;

            var vm = rd.View.DataContext as PackingViewModel;
            if (vm == null)
                return false;

            if (vm.ActivePack == null)
                return false;

            return te.TECode == vm.ActivePack.TECode;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }


    public class IsLastPackedProductConverter : MarkupExtension, IMultiValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var product = values[0] as Product;
            if (product == null)
                return false;

            var rd = values[1] as RowData;
            if (rd == null)
                return false;
            
            var rowProduct = rd.Row as Product;
            if (rowProduct == null || rowProduct.ProductId != product.ProductId)
                return false;

            var vm = rd.View.DataContext as PackingViewModel;
            if (vm == null)
                return false;

            return vm.ActivePack != null && string.Equals(vm.ActivePack.GetKey(), rowProduct.TECode);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public sealed class NullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DefaultBoolean.False;

            var te = value as TE;
            if (te == null)
                return DefaultBoolean.False;

            if (te.TEPackStatus.Equals(TEPackStatus.TE_PKG_ACTIVATED))
                return DefaultBoolean.True;

            return DefaultBoolean.False;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
