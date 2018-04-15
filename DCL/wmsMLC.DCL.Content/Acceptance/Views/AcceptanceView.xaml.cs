using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.TreeList;
using wmsMLC.DCL.Content.Acceptance.ViewModels;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Resources;
using wmsMLC.General.PL.WPF;

namespace wmsMLC.DCL.Content.Acceptance.Views
{
    public partial class AcceptanceView : IHelpHandler
    {
        private AcceptanceViewModel ViewModel
        {
            get { return (AcceptanceViewModel) DataContext; }
        }

        #region .  Methods  .
        public AcceptanceView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            PlaceEntityRefEdit.Loaded += PlaceEntityRefEditOnLoaded;

            // Выставить Image из ресурсов в xaml оказалось проблемой (нужно было делать Bitmap2ImageSourceConverter)
            SetAcceptImage();
            SetProductsImage();
        }

        private void PlaceEntityRefEditOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            PlaceEntityRefEdit.Loaded -= PlaceEntityRefEditOnLoaded;

            if (ViewModel != null)
                ViewModel.SetFields(PlaceEntityRefEdit.ViewModel);
        }

        private void SetProductsImage()
        {
            imgProducts.BeginInit();
            imgProducts.Source = ImageResources.DCLFilter32.GetBitmapImage();
            imgProducts.EndInit();
        }

        private void SetAcceptImage()
        {
            imgAccept.BeginInit();
            imgAccept.Source = ImageResources.DCLAccept32.GetBitmapImage();
            imgAccept.EndInit();
        }

        private void SubscribeSource()
        {
            UnSubscribeSource();

            ViewModel.SourceUpdateStarted += OnSourceUpdateStarted;
            ViewModel.SourceUpdateCompleted += OnSourceUpdateCompleted;
        }

        private void UnSubscribeSource()
        {
            ViewModel.SourceUpdateStarted -= OnSourceUpdateStarted;
            ViewModel.SourceUpdateCompleted -= OnSourceUpdateCompleted;
        }

        //private async void PlaceLookUpEditOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        //{
        //    // запоминаем выбранное место
        //    var selectedPlace = ViewModel.SelectedPlace;
        //    var control = ((CustomLookUpEdit) sender);

        //    // получаем данные
        //    await control.RefreshData(true);

        //    // восстанавливаем умолчания
        //    var collection = control.ItemsSource as IEnumerable<Place>;
        //    if (collection != null)
        //    {
        //        // если места нет - берем перовое попавшееся из списка
        //        ViewModel.SelectedPlace = selectedPlace != null
        //            ? collection.FirstOrDefault(i => i.PlaceCode == selectedPlace.PlaceCode)
        //            : collection.FirstOrDefault();
        //        //placeLookUpEdit.EditValue = ViewModel.SelectedPlace;
        //        placeLookUpEdit.EditValueChanged += (o, args) =>
        //        {
                    
        //        };
        //    }

        //}

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

        private void OnSourceUpdateStarted(object sender, EventArgs eventArgs)
        {
            // т.к. мы не знаем откуда пришло событие - на всякий случай делаем безопасный вызов
            //objectListGridControl.Dispatcher.Invoke(new Action(() => objectListGridControl.BeginDataUpdate()), DispatcherPriority.DataBind);
        }

        private void OnSourceUpdateCompleted(object sender, EventArgs eventArgs)
        {
            // т.к. мы не знаем откуда пришло событие - на всякий случай делаем безопасный вызов
            //objectListGridControl.Dispatcher.Invoke(new Action(() => objectListGridControl.EndDataUpdate()), DispatcherPriority.DataBind);
        }

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

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var vm = DataContext as ICustomListViewModel;
            if (vm != null)
            {
                vm.InitializeMenus();
                //ViewWorking.ParentViewModel = vm;
            }

            var vmp = DataContext as AcceptanceViewModel;
            if (vmp != null)
            {
                //ViewWorking.Fields = vmp.SubFields;
                vmp.CheckIsEnabled();
            }

            SubscribeSource();
        }

        //private void ViewWorking_OnSelectionChanged(object sender, EventArgs e)
        //{
        //    var selectedItems = ViewWorking.SelectedItems;
        //    if (selectedItems == null || selectedItems.Count == 0)
        //        return;

        //    var working = selectedItems.Cast<Working>().FirstOrDefault();
        //    if (working == null)
        //        return;

        //    using (var mgr = IoC.Instance.Resolve<IBaseManager<Work>>())
        //    {
        //        var work = mgr.Get(working.WORKID_R);
        //        if (work != null)
        //            ViewWorking.ParentViewModelSource = work;
        //    }
        //}

        #endregion

        #region . IDisposable .
        //protected override void Dispose(bool disposing)
        //{
        //    // events
        //    UnSubscribeSource();
        //    DataContextChanged -= OnDataContextChanged;

        //    // найдем и удалим все CustomComboBoxEdit
        //    var comboList = FindChilds(objectListGridControl, typeof(CustomComboBoxEdit));
        //    foreach (var combo in comboList)
        //    {
        //        var disposable = combo as IDisposable;
        //        if (disposable != null)
        //            disposable.Dispose();
        //    }

        //    // найдем и удалим все CustomComboBoxEdit
        //    var lookupList = FindChilds(objectListGridControl, typeof(CustomLookUpEdit));
        //    foreach (var lookup in lookupList)
        //    {
        //        var disposable = lookup as IDisposable;
        //        if (disposable != null)
        //            disposable.Dispose();
        //    }

        //    base.Dispose(disposing);
        //}
        #endregion

        private void OnShowingEditor(object sender, TreeListShowingEditorEventArgs e)
        {
            var viewModel = (AcceptanceViewModel) DataContext;
            if (e.Node != null)
                e.Cancel = e.Handled = !viewModel.IsAllowEditProperty(e.Column.FieldName, e.Node.Content);
        }

        /// <summary>
        /// Раскрываем ветку дерева при добавлении нового объекта 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeListView_OnNodeChanged(object sender, TreeListNodeChangedEventArgs e)
        {
            if (e.ChangeType != NodeChangeType.Add || e.Node.ParentNode == null)
                return;

            var parent = e.Node.ParentNode;
            if (!parent.IsExpanded)
                parent.ExpandAll();
        }
    }
}
