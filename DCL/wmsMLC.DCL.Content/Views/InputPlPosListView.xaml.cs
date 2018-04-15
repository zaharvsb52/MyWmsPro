using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.Content.ViewModels;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Main.Views.Controls;
using wmsMLC.DCL.Resources;

namespace wmsMLC.DCL.Content.Views
{
    public partial class InputPlPosListView : DXPanelView, IHelpHandler
    {
        public InputPlPosListView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = DataContext as InputPlPosListViewModel;
            if (vm != null)
            {
                vm.InitializeMenus();

                foreach (var bar in ViewWorking.Menu.Bars)
                {
                    foreach (var menu in bar.MenuItems.Where(p => p.Caption == StringResources.MenuCaptionEditTable).ToArray())
                    {
                        bar.MenuItems.Remove(menu);
                    }
                }

                ViewWorking.IsGridEditMode = false;
                ViewWorking.ParentViewModel = vm;
                ViewWorking.CustomEditModel = () =>
                {
                    var model = new InputPlPosViewModel {PlId = vm.GetPlId()};
                    return model;
                };
                ViewWorking.ShouldUpdateSeparately = true;
                ViewWorking.IsReadOnly = vm.IsReadOnly;
                ViewWorking.SetItemType(vm.SublistViewItemType);
                ViewWorking.Fields = vm.GetSublistViewFields();
            }

            SubscribeSource();
        }

        private void SubscribeSource()
        {
            UnSubscribeSource();

            var vm = DataContext as InputPlPosListViewModel;
            if (vm == null)
                return;

            vm.SourceUpdateStarted += OnSourceUpdateStarted;
            vm.SourceUpdateCompleted += OnSourceUpdateCompleted;
            vm.PropertyChanged += OnVmPropertyChanged;
        }

        private void UnSubscribeSource()
        {
            var vm = DataContext as InputPlPosListViewModel;
            if (vm == null)
                return;

            vm.SourceUpdateStarted -= OnSourceUpdateStarted;
            vm.SourceUpdateCompleted -= OnSourceUpdateCompleted;
            vm.PropertyChanged -= OnVmPropertyChanged;
        }

        private void OnVmPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == InputPlPosListViewModel.IsReadOnlyPropertyName)
            {
                var vm = DataContext as InputPlPosListViewModel;
                if (vm != null)
                    ViewWorking.IsReadOnly = vm.IsReadOnly;
            }
        }

        private void OnSourceUpdateCompleted(object sender, EventArgs eventArgs)
        {
            // т.к. мы не знаем откуда пришло событие - на всякий случай делаем безопасный вызов
            objectListGridControl.Dispatcher.Invoke(new Action(() => objectListGridControl.EndDataUpdate()), DispatcherPriority.DataBind);
        }

        private void OnSourceUpdateStarted(object sender, EventArgs eventArgs)
        {
            // т.к. мы не знаем откуда пришло событие - на всякий случай делаем безопасный вызов
            objectListGridControl.Dispatcher.Invoke(new Action(() => objectListGridControl.BeginDataUpdate()), DispatcherPriority.DataBind);
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
            return "CustomList";
        }

        string IHelpHandler.GetHelpEntity()
        {
            var dc = DataContext as IHelpHandler;
            return dc == null ? string.Empty : dc.GetHelpEntity();
        }

        #region . IDisposable .
        protected override void Dispose(bool disposing)
        {
            // events
            UnSubscribeSource();
            DataContextChanged -= OnDataContextChanged;

            // найдем и удалим все CustomComboBoxEdit
            var comboList = FindChilds(objectListGridControl, typeof(CustomComboBoxEdit));
            foreach (var combo in comboList)
            {
                var disposable = combo as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }

            // найдем и удалим все CustomComboBoxEdit
            var lookupList = FindChilds(objectListGridControl, typeof(CustomLookUpEdit));
            foreach (var lookup in lookupList)
            {
                var disposable = lookup as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion

        public static IEnumerable<DependencyObject> FindChilds(DependencyObject obj, Type type)
        {
            if (obj != null)
            {
                if (obj.GetType() == type)
                {
                    yield return obj;
                }

                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
                {
                    foreach (var child in FindChilds(VisualTreeHelper.GetChild(obj, i), type))
                    {
                        if (child != null)
                        {
                            yield return child;
                        }
                    }
                }
            }
        }

        private void TableView_ShowingEditor(object sender, ShowingEditorEventArgs e)
        {
            e.Cancel = !IsEnabledEdit(e.Row, e.Column.FieldName);
        }

        private void OnViewWorkingSelectionChanged(object sender, EventArgs e)
        {
            var selectedItems = ViewWorking.SelectedItems;
            if (selectedItems == null || selectedItems.Count == 0)
                return;

            var working = selectedItems.Cast<WMSBusinessObject>().FirstOrDefault();
            if (working == null)
                return;

            var workid = working.GetProperty<decimal?>(Working.WORKID_RPropertyName);
            if (!workid.HasValue)
                return;

            var vm = DataContext as InputPlPosListViewModel;
            if (vm == null || !vm.Works.ContainsKey(workid.Value))
                return;

            ViewWorking.ParentViewModelSource = vm.Works[workid.Value];
        }

        private void OnShowGridMenu(object sender, GridMenuEventArgs e)
        {
            if (e.MenuType != GridMenuType.RowCell) 
                return;

            if (e.Items == null || e.Items.Count == 0)
                return;

            foreach (var item in e.Items)
            {
                item.IsVisible = false;
            }

            if (!CanEdit())
                return;

            var cells = GetSelectedCells();
            if (cells == null || cells.Count == 0)
                return;

            var columns =
                cells.Where(c => c.Column != null && !string.IsNullOrEmpty(c.Column.FieldName))
                    .Select(c => c.Column.FieldName)
                    .Distinct()
                    .ToArray();
            if (columns.Length != 1 || columns[0] != InputPlPos.InputplpostemanPropertyName)
                return;

            e.Items[1].IsVisible = true;

            object cellvalue;
            if (!ValidateCells(cells, out cellvalue) || cellvalue == null || Equals(cellvalue, string.Empty))
                return;

            e.Items[0].IsVisible = true;
            e.Items[0].Content = string.Format(StringResources.AutoValueinsertFormat, cellvalue);
        }

        private void OnAutoinsertItemClick(object sender, ItemClickEventArgs e)
        {
            if (!CanEdit())
                return;

            var cells = GetSelectedCells();
            if (cells == null || cells.Count == 0)
                return;

            object cellvalue = null;
            if (Equals(e.Item.CommandParameter, "0"))
            {
                if (!ValidateCells(cells, out cellvalue) || cellvalue == null || Equals(cellvalue, string.Empty))
                    return;
            }

            foreach (var cell in cells)
            {
                if (!IsEnabledEdit(objectListGridControl.GetRow(cell.RowHandle), cell.Column.FieldName))
                    continue;

                objectListGridControl.SetCellValue(cell.RowHandle, cell.Column, cellvalue);
            }
        }

        private bool ValidateCells(IList<GridCell> cells, out object value)
        {
            value = null;
            if (cells == null || cells.Count <= 1)
                return false;

            var selcell = cells[0];
            value = objectListGridControl.GetCellValue(selcell.RowHandle, selcell.Column);
            if (value == null || Equals(value, string.Empty))
                return false;

            return true;
        }

        private bool IsEnabledEdit(object entity, string propertyName)
        {
            var valueEditController = DataContext as IValueEditController;
            return valueEditController != null && valueEditController.EnableEdit(entity, propertyName);
        }

        private IList<GridCell> GetSelectedCells()
        {
            var view = objectListGridControl.View as TableView;
            if (view == null)
                return null;

            return view.GetSelectedCells();
        }

        private bool CanEdit()
        {
            var valueEditController = DataContext as IValueEditController;
            return valueEditController != null && valueEditController.CanEdit();
        }
    }
}
