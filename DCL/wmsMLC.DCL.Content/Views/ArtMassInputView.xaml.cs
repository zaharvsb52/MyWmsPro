using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using DevExpress.Utils;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.Content.ViewModels;
using wmsMLC.DCL.Content.ViewModels.ArtMassInput;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Main.Views.Controls;
using wmsMLC.General.PL.Model;
using ColumnTemplateSelector = wmsMLC.DCL.Main.Helpers.ColumnTemplateSelector;
using DevExpress.Xpf.Core.Native;

namespace wmsMLC.DCL.Content.Views
{
    /// <summary>
    /// Interaction logic for ArtMassInputView.xaml
    /// </summary>
    public partial class ArtMassInputView : DXPanelView
    {
        readonly Dictionary<CustomGridControl, GridExpander> _gridExpanders = new Dictionary<CustomGridControl, GridExpander>();

        public ArtMassInputView()
        {
            InitializeComponent();
            Unloaded += (sender, args) =>
            {
                foreach (var gridExpander in _gridExpanders)
                {
                    gridExpander.Value.StopObserving();
                }
                _gridExpanders.Clear();
            };
        }
        
        private void AddExpander(CustomGridControl grid)
        {
            var ctx = (IMassInputListViewModel)grid.DataContext;

            if (ctx == null)
                return;

            GridExpander expander;
            if (_gridExpanders.TryGetValue(grid, out expander))
            {
                expander.StopObserving();
                _gridExpanders.Remove(grid);
            }

            expander = new GridExpander(grid, ctx);
            _gridExpanders[grid] = expander;
            expander.StartObserving();
        }

        private void List_OnInitialized(object sender, EventArgs e)
        {
            var grid = (CustomGridControl)sender;
            ((ColumnTemplateSelector)grid.ColumnGeneratorTemplateSelector).AllowUseComboBoxAsEditor = true;

            // Обновить колонки, если  уже отрисованы
            if (grid.ColumnsSource != null)
            {
                var temp = grid.ColumnsSource;
                grid.ColumnsSource = null;
                grid.ColumnsSource = temp;
            }
        }

        private void SKUList_OnInitializedList_OnInitialized(object sender, EventArgs e)
        {
            var grid = (CustomGridControl)sender;
            var columnTemplateSelector = (ColumnTemplateSelector)grid.ColumnGeneratorTemplateSelector;
            
            columnTemplateSelector.AllowUseComboBoxAsEditor = true;
            columnTemplateSelector.CustomColumn += SKUListCustomColumn;
            // Обновить колонки, если  уже отрисованы
            if (grid.ColumnsSource != null)
            {
                var temp = grid.ColumnsSource;
                grid.ColumnsSource = null;
                grid.ColumnsSource = temp;
            }
        }

        private void SKUListCustomColumn(object sender, ColumnTemplateSelector.CustomColumnEventArgs customColumnEventArgs)
        {
            if (customColumnEventArgs.DataField.FieldName != SKU.SKUParentPropertyName)
                return;

            customColumnEventArgs.ColumnTemplate = (DataTemplate) FindResource("SKUParentColumnTemplate");
        }

        private void List_OnSelectedItemChanged(object sender, SelectedItemChangedEventArgs e)
        {
            var grid = (CustomGridControl)e.Source;
            var ctx = (IMassInputListViewModel)grid.DataContext;
            if (ctx == null)
                return; // так и не понял при каких обстоятельствах

            ctx.SetSelectedItem(e.NewItem);
        }

        private class GridExpander
        {
            private readonly CustomGridControl _grid;
            private readonly IMassInputListViewModel _viewModel;

            public GridExpander(CustomGridControl grid, IMassInputListViewModel viewModel)
            {
                if (grid == null)
                    throw new ArgumentNullException("grid");
                
                if(viewModel == null)
                    throw new ArgumentNullException("viewModel");

                _grid = grid;
                _viewModel = viewModel;
            }

            public void StartObserving()
            {
                _viewModel.ItemAdded += SetFocusAndExpandItem;
            }

            public void StopObserving()
            {
                _viewModel.ItemAdded -= SetFocusAndExpandItem;
            }

            private void SetFocusAndExpandItem(object sender, AddItemEventArgs e)
            {
                var view = (TableView) _grid.View;
                view.Focus();

                _grid.CurrentItem = e.Item;
                _grid.CurrentColumn = _grid.Columns.First(c => c.AllowEditing == DefaultBoolean.True);

                if (_grid.AllowMasterDetail)
                {
                    _grid.SetMasterRowExpanded(view.FocusedRowHandle, true);
                }

                Application.Current.Dispatcher.BeginInvoke(new Action(view.ShowEditor), DispatcherPriority.Loaded);
            }
        }

        #region Art header
        private void FillArtGroup(ArtItem art, CustomDataLayoutControl group)
        {
            NameScope.SetNameScope(group, new NameScope());
            var fields = art.Fields;
            foreach (var field in fields)
            {
                var layoutItem = new CustomDataLayoutItem(field)
                {
                    IsVisibilitySetOutside = true,
                    IsDisplayFormatSetOutside = true,
                    ToolTipIns = CreateCustomSuperToolTip(field)
                };

                group.Children.Add(layoutItem);
            }
            group.UpdateLayout();
        }

        private static StackPanel CreateCustomSuperToolTip(DataField field)
        {
            var stack = new StackPanel();
            stack.Children.Add(new TextBlock { Text = field.Caption });
            stack.Children.Add(new SuperTipItemControlSeparator());
            stack.Children.Add(new TextBlock { Text = field.Description });
            stack.Children.Add(new SuperTipItemControlSeparator());
            stack.Children.Add(new TextBlock { Text = string.Format("[{0}]", field.Name), FontFamily = new FontFamily("Segoe UI"), Foreground = Brushes.Gray, FontSize = 11 });
            return stack;
        }

        private void ArtHeader_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;

            var dataLayoutControl = (CustomDataLayoutControl)sender;
            var artItem = (ArtItem)e.NewValue;
            FillArtGroup(artItem, dataLayoutControl);
            dataLayoutControl.RestoreLayout(
"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
"<CustomDataLayoutControl ID=\"ArtHeader\">" +
"<LayoutGroup>" +
"<Element ID=\"ARTNAME\" Label=\"\" />" +
"<Element ID=\"MANDANTID\" Label=\"\" />" +
"</LayoutGroup>" +
"<Element ID=\"ARTDESC\" Label=\"\" />" +
"<LayoutGroup>" +
"<Element ID=\"ARTABCD\" Label=\"\" />" +
"<Element ID=\"FACTORYID_R\" Label=\"\" />" +
"</LayoutGroup>" +
"<LayoutGroup>" +
"<Element ID=\"ARTINPUTDATEMETHOD\" Label=\"\" />" +
"<Element ID=\"ARTCOMMERCDAY\" Label=\"\" />" +
"</LayoutGroup>" +
"<LayoutGroup>" +
"<Element ID=\"ARTLIFETIME\" Label=\"\" />" +
"<Element ID=\"ARTLIFETIMEMEASURE\" Label=\"\" />" +
"</LayoutGroup>" +
"<LayoutGroup>" +
"<Element ID=\"ARTIWBTYPE\" Label=\"\" />" +
"<Element ID=\"ARTPICKORDER\" Label=\"\" />" +
"</LayoutGroup>" +
"<AvailableItems>" +
"<Element ID=\"ARTDESCEXT\" Label=\"\" />" +
"<Element ID=\"ARTCOLOR\" Label=\"\" />" +
"<Element ID=\"ARTCOLORTONE\" Label=\"\" />" +
"<Element ID=\"ARTSIZE\" Label=\"\" />" +
"<Element ID=\"ARTTEMPMIN\" Label=\"\" />" +
"<Element ID=\"ARTTEMPMAX\" Label=\"\" />" +
"<Element ID=\"ARTDANGERLEVEL\" Label=\"\" />" +
"<Element ID=\"ARTDANGERSUBLEVEL\" Label=\"\" />" +
"<Element ID=\"ARTHOSTREF\" Label=\"\" />" +
"</AvailableItems>" +
"</CustomDataLayoutControl>");
        }

        #endregion

        private void List_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "VisibleRowCount")
                return;

            var grid = (CustomGridControl)sender;
            AddExpander(grid);
        }

        private void GridView_OnCellValueChanging(object sender, CellValueChangedEventArgs e)
        {
            var view = sender as TableView;
            if (view != null) 
                view.PostEditor();
        }

//        public decimal? CurrentMandantId { get; set; }
//
//        private void ArtList_OnShownEditor(object sender, EditorEventArgs e)
//        {
//            if (e.Column.FieldName != "FACTORYID_R")
//                return;
//
//            ComboBoxEdit lookup = e.Editor as ComboBoxEdit;
//            if (lookup == null)
//                return;
//
//            TableView view = (TableView)sender;
//            CurrentMandantId = (decimal?)view.Grid.GetCellValue(e.RowHandle, "MANDANTID");
//            lookup.PopupOpened -= FactoryLookupPopupOpened;
//            lookup.PopupOpened += FactoryLookupPopupOpened;
//        }
//
//        public PopupListBox CurrentFactoryPopupListBox { get; set; }
//        private void FactoryLookupPopupOpened(object sender, RoutedEventArgs routedEventArgs)
//        {
//            PopupListBox lb = (PopupListBox)LookUpEditHelper.GetVisualClient((ComboBoxEdit)sender).InnerEditor;
//            CurrentFactoryPopupListBox = lb;
//            lb.LayoutUpdated -= LbOnLayoutUpdated;
//            lb.LayoutUpdated += LbOnLayoutUpdated;
//        }
//
//        private void LbOnLayoutUpdated(object sender, EventArgs eventArgs)
//        {
//            for (var i = 0; i < CurrentFactoryPopupListBox.Items.Count; i++)
//            {
//                Factory f = (Factory)CurrentFactoryPopupListBox.Items[i];
//                ComboBoxEditItem item = (ComboBoxEditItem)CurrentFactoryPopupListBox.ItemContainerGenerator.ContainerFromItem(f);
//                if (item == null)
//                    continue;
//
//                item.IsEnabled = (f.PARTNERID_R == null) || (CurrentMandantId.HasValue && f.PARTNERID_R.Value == CurrentMandantId.Value);
//            }
//        }
    }
}
