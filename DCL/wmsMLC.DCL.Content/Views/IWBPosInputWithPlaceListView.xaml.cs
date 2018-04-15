using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.Content.ViewModels;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Main.Views.Controls;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.DCL.Content.Views
{
    /// <summary>
    /// Interaction logic for IWBPosInputListView.xaml
    /// </summary>
    public partial class IWBPosInputWithPlaceListView : DXPanelView, IHelpHandler
    {
        #region .  Methods  .
        public IWBPosInputWithPlaceListView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;

            ViewWorking.IsGridEditMode = false;
            ViewWorking.ShouldUpdateSeparately = true;
            ViewWorking.SetItemType(typeof(Working));
        }

        private void SubscribeSource()
        {
            var vm = DataContext as IWBPosInputWithPlaceListViewModel;
            if (vm == null)
                return;

            placeLookUpEdit.LookUpCodeEditor = "PLACE_PLACECODE";
            placeLookUpEdit.LookUpCodeEditorFilterExt = vm.PlaceFilter;
            placeLookUpEdit.SourceChanged += placeLookUpEdit_SourceChanged;

            vm.SourceUpdateStarted -= OnSourceUpdateStarted;
            vm.SourceUpdateCompleted -= OnSourceUpdateCompleted;

            vm.SourceUpdateStarted += OnSourceUpdateStarted;
            vm.SourceUpdateCompleted += OnSourceUpdateCompleted;
        }

        private void UnSubscribeSource()
        {
            var vm = DataContext as IModelHandler;
            if (vm == null)
                return;

            vm.SourceUpdateStarted -= OnSourceUpdateStarted;
            vm.SourceUpdateCompleted -= OnSourceUpdateCompleted;

            placeLookUpEdit.SourceChanged -= placeLookUpEdit_SourceChanged;
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

        public async override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            await placeLookUpEdit.RefreshData(false);
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

        private void placeLookUpEdit_SourceChanged(object sender, RoutedEventArgs e)
        {
            Place currentPlace = null;
            var vm = DataContext as IWBPosInputWithPlaceListViewModel;
            if (vm != null)
                currentPlace = vm.CurrentPlace;

            var collection = sender as IEnumerable;
            if (collection != null)
            {
                if (currentPlace != null)
                    currentPlace = collection.Cast<Place>().FirstOrDefault(i => i.GetKey().Equals(currentPlace.GetKey()));

                currentPlace = currentPlace ?? collection.Cast<Place>().FirstOrDefault();
                if (vm != null)
                    vm.CurrentPlace = currentPlace;
                placeLookUpEdit.EditValue = currentPlace;
            }
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
                ViewWorking.ParentViewModel = vm;
            }

            var vmp = DataContext as IWBPosInputWithPlaceListViewModel;
            if (vmp != null)
            {
                ViewWorking.Fields = vmp.SubFields;
                vmp.CheckIsEnabled();
            }

            SubscribeSource();
        }

        private void ViewWorking_OnSelectionChanged(object sender, EventArgs e)
        {
            var selectedItems = ViewWorking.SelectedItems;
            if (selectedItems == null || selectedItems.Count == 0)
                return;

            var working = selectedItems.Cast<Working>().FirstOrDefault();
            if (working == null)
                return;

            using (var mgr = IoC.Instance.Resolve<IBaseManager<Work>>())
            {
                var work = mgr.Get(working.WORKID_R);
                if (work != null)
                    ViewWorking.ParentViewModelSource = work;
            }
        }

        #endregion

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
    }
}
