using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using DevExpress.Xpf.Grid;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views.Controls;
using wmsMLC.General;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.DCL.Main.Views
{
    public partial class CustomLookUpOptPopupContent : IDisposable
    {
        public CustomLookUpOptPopupContent()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
            KeyDown += OnKeyDown;
            Loaded += OnLoaded;
        }

        public bool IsDisposed { get; private set; }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnLoaded;
            var viewservice = IoC.Instance.Resolve<IViewService>();
            viewservice.RestoreLayout(this);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                DialogResult = false;
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            SubscribeSource();
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

            var lm = vm as IObjectListViewModel;
            if (lm != null)
                lm.ShouldChangeSelectedItem -= OnShouldChangeSelectedItem;
        }

        private void OnShouldChangeSelectedItem(object sender, EventArgs e)
        {
            SelectGridItem(objectListGridControl);
        }

        private void OnSourceUpdateStarted(object sender, EventArgs eventArgs)
        {
            // т.к. мы не знаем откуда пришло событие - на всякий случай делаем безопасный вызов
            objectListGridControl.Dispatcher.Invoke(new Action(() => objectListGridControl.BeginDataUpdate()), DispatcherPriority.DataBind);
        }

        private void OnSourceUpdateCompleted(object sender, EventArgs eventArgs)
        {
            // т.к. мы не знаем откуда пришло событие - на всякий случай делаем безопасный вызов
            objectListGridControl.Dispatcher.Invoke(new Action(() => objectListGridControl.EndDataUpdate()), DispatcherPriority.DataBind);
        }

        private void OnItemsSourceChanged(object sender, ItemsSourceChangedEventArgs e)
        {
            if (e.NewItemsSource == null || DataContext == null)
                return;

            var grid = sender as CustomGridControl;
            SelectGridItem(grid);
        }

        private void ORestoredLayoutFromXml(object sender, EventArgs e)
        {
            var grid = sender as CustomGridControl;
            SelectGridItem(grid);
        }

        private void SelectGridItem(CustomGridControl grid)
        {
            if (grid == null)
                return;

            grid.UnselectAll();

            var model = GetModel();
            if (model == null || model.EditValue == null)
                return;

            var itemsSource = grid.ItemsSource as IList;
            if (itemsSource == null)
                return;

            Func<object, string, WMSBusinessObject, bool> compareFunc = (val, valMember, row) =>
            {
                if (row == null)
                    return false;
                if (string.IsNullOrEmpty(valMember))
                    return val.Equals(row.GetKey());
                return val.Equals(row.GetProperty(valMember));
            };

            try
            {
                grid.BeginSelection();
                var index = -1;
                for (var i = 0; i < grid.RowsCount; i++)
                {
                    var obj = grid.GetRow(i) as WMSBusinessObject;
                    if (compareFunc(model.EditValue, model.ValueMember, obj))
                    {
                        index = i;
                        break;
                    }
                }

                if (index < 0)
                    return;

                grid.SelectItem(index);
                var tableView = grid.View as TableView;
                if (tableView != null)
                {
                    tableView.FocusedRowHandle = index;
                    tableView.MoveFocusedRow(tableView.FocusedRowHandle);
                }
            }
            finally
            {
                grid.EndSelection();
            }
        }

        private IObjectListViewModel GetModel()
        {
            return DataContext as IObjectListViewModel;
        }

        private void OnSelectButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        public static IEnumerable<DependencyObject> FindChilds(DependencyObject obj, Type type)
        {
            if (obj == null)
                yield break;

            if (obj.GetType() == type)
                yield return obj;

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                foreach (var child in FindChilds(VisualTreeHelper.GetChild(obj, i), type))
                {
                    if (child != null)
                        yield return child;
                }
            }
        }

        private void ObjectListGridControl_OnPreviewKeyDown(object sender, KeyEventArgs e)
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

        #region .  IDisposable  .
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // events
                UnSubscribeSource();
                DataContextChanged -= OnDataContextChanged;
                KeyDown -= OnKeyDown;
                Loaded -= OnLoaded;

                // найдем и удалим все CustomComboBoxEdit
                var comboList = FindChilds(objectListGridControl, typeof (CustomComboBoxEdit));
                foreach (var combo in comboList)
                {
                    var disposable = combo as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }

                // найдем и удалим все CustomComboBoxEdit
                var lookupList = FindChilds(objectListGridControl, typeof (CustomLookUpEdit));
                foreach (var lookup in lookupList)
                {
                    var disposable = lookup as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }

                if (!IsDisposed)
                {
                    // спрашиваем ViewModel
                    var disposable = DataContext as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                    IsDisposed = true;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion .  IDisposable  .
    }
}
