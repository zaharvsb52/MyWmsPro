using System;
using System.Collections;
using System.IO;
using System.Windows;
using System.Windows.Input;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views.Controls;
using wmsMLC.General;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.DCL.Main.Views
{
    public partial class ObjectListView : DXPanelView, IHelpHandler
    {
        public const string DefaultHelpLink = "List";

        public ObjectListView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        #region .  Methods  .
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            SubscribeSource();
            var vm = DataContext as IListViewModel;
            if (vm != null)
                DispatcherHelper.Invoke(new Action(vm.InitializeMenus));
        }

        private void SubscribeSource()
        {
            UnSubscribeSource();

            var vm = DataContext as IModelHandler;
            if (vm != null)
            {
                vm.SourceUpdateStarted += OnSourceUpdateStarted;
                vm.SourceUpdateCompleted += OnSourceUpdateCompleted;
            }

            var vme = DataContext as IExportData;
            if (vme != null)
                vme.SourceExport += OnSourceExport;

            var lm = vm as IObjectListViewModel;
            if (lm != null)
                lm.ShouldChangeSelectedItem += OnShouldChangeSelectedItem;
        }

        private void UnSubscribeSource()
        {
            var vm = DataContext as IModelHandler;
            if (vm != null)
            {
                vm.SourceUpdateStarted -= OnSourceUpdateStarted;
                vm.SourceUpdateCompleted -= OnSourceUpdateCompleted;
            }

            var vme = DataContext as IExportData;
            if (vme != null)
                vme.SourceExport -= OnSourceExport;

            var lm = vm as IObjectListViewModel;
            if (lm != null)
                lm.ShouldChangeSelectedItem -= OnShouldChangeSelectedItem;
        }

        private void OnShouldChangeSelectedItem(object sender, EventArgs e)
        {
            SelectGridItem(objectListGridControl);
        }

        private void OnSourceUpdateCompleted(object sender, EventArgs eventArgs)
        {
            // т.к. мы не знаем откуда пришло событие - на всякий случай делаем безопасный вызов
            DispatcherHelper.BeginInvoke(new Action(() => objectListGridControl.EndDataUpdate()));
            //DispatcherHelper.BeginInvoke(new Action(() => objectListGridControl.SelectedItems.Clear()));
        }

        private void OnSourceUpdateStarted(object sender, EventArgs eventArgs)
        {
            // т.к. мы не знаем откуда пришло событие - на всякий случай делаем безопасный вызов
            DispatcherHelper.BeginInvoke(new Action(() => objectListGridControl.BeginDataUpdate()));
        }

        private void OnSourceExport(object sender, EventArgs eventArgs)
        {
            var vme = DataContext as IExportData;
            if (vme == null)
                return;

            var stream = new MemoryStream();
            objectListGridControl.SaveLayoutToStream(stream);

            vme.StreamExport = stream;
        }

        public override bool CanClose()
        {
            var res = base.CanClose();
            if (res)
                UnSubscribeSource();

            return res;
        }

        private void SelectGridItem(CustomGridControl grid)
        {
            if (grid == null)
                return;

            var selectionItems = grid.SelectedItems;
            var itemsSource = grid.ItemsSource as IList;

            if (itemsSource == null || selectionItems == null)
                return;

            var tableView = grid.View as TableView;
            if (tableView == null)
                return;

            // если ничего не выбрано - нужно сбрасывать focus (в противном случае при сортировках записи скачут)
            if (selectionItems.Count == 0)
            {
                var mv = DataContext as IListViewModel;
                if (mv != null && mv.IsSelectedFirstItem)
                {
                    grid.UnselectAll();
                    grid.SelectItem(0);
                    MoveFocusedRow(tableView, 0);
                    return;
                }

                tableView.FocusedRowHandle = -1;
                return;
            }

            try
            {
                grid.BeginSelection();
                grid.UnselectAll();

                foreach (var item in selectionItems)
                {
                    var ikh = item as IKeyHandler;
                    if (ikh == null)
                        continue;

                    var index = grid.IndexOf(ikh.GetKey());
                    if (index < 0)
                        continue;

                    grid.SelectItem(index);
                    MoveFocusedRow(tableView, index);
                }
            }
            finally
            {
                grid.EndSelection();
            } 
        }

        private void MoveFocusedRow(TableView tableView, int index)
        {
            if (tableView == null)
                return;

            tableView.FocusedRowHandle = index;
            tableView.MoveFocusedRow(index);
        }

        private void OnItemsSourceChanged(object sender, ItemsSourceChangedEventArgs e)
        {
            if (e.NewItemsSource == null)
                return;

            var grid = sender as CustomGridControl;
            if (grid == null)
                return;

            SelectGridItem(grid);
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // если мы сейчас в режиме редактирования фильтра и нажали Enter, то нужно получить данные
            //TODO: Добавить проверку на нахождение в строке автопоиска (вот такая не работает - objectListGridControl.View.FocusedRowHandle == DevExpress.Data.CurrencyDataController.FilterRow)
            if (e.Key == Key.Enter
                && objectListGridControl.View.FocusedRowHandle < 0)
            {
                var listModel = DataContext as IListViewModel;
                if (listModel != null)
                {
                    // заставляем вычитать данные
                    listModel.ApplyFilter();
                    e.Handled = true;
                }
            }
        }

        private void OnRestoredLayoutFromXml(object sender, EventArgs e)
        {
            var mv = DataContext as IListViewModel;
            if (mv != null && mv.IsSelectedFirstItem)
            {
                var grid = sender as CustomGridControl;
                if (grid == null || grid.RowsCount <= 0)
                    return;

                var tableView = grid.View as TableView;
                if (tableView == null)
                    return;

                grid.UnselectAll();
                grid.SelectItem(0);
                MoveFocusedRow(tableView, 0);
            }
        }

        private void Copy_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var oldMode = objectListGridControl.ClipboardCopyMode;
            try
            {
                var mode = (ClipboardCopyMode)Enum.Parse(typeof(ClipboardCopyMode), e.Item.CommandParameter.ToString());
                objectListGridControl.ClipboardCopyMode = mode;
                objectListGridControl.ByCheckCopyMode = false;
                objectListGridControl.CopyToClipboard();
            }
            finally
            {
                objectListGridControl.ClipboardCopyMode = oldMode;
                objectListGridControl.ByCheckCopyMode = true;
            }
        } 
        #endregion

        #region .  IHelpHandler  .
        string IHelpHandler.GetHelpLink()
        {
            var dc = DataContext as IHelpHandler;
            if (dc != null)
            {
                var link = dc.GetHelpLink();
                if (link != null)
                    return link;
            }
            return DefaultHelpLink;
        }

        string IHelpHandler.GetHelpEntity()
        {
            var dc = DataContext as IHelpHandler;
            return dc == null ? string.Empty : dc.GetHelpEntity();
        }
        #endregion

        #region .  IDisposable  .
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // events
                UnSubscribeSource();
                DataContextChanged -= OnDataContextChanged;

                // прибиваем грид
                objectListGridControl.Dispose();

                // прибиваем фильтр
                filter.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
