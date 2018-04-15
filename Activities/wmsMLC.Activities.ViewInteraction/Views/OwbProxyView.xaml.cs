using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using wmsMLC.Activities.ViewInteraction.ViewModels;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Main.Views.Controls;

namespace wmsMLC.Activities.ViewInteraction.Views
{
    /// <summary>
    /// Interaction logic for OwbProxyView.xaml
    /// </summary>
    public partial class OwbProxyView : DXPanelView, IHelpHandler
    {
        public OwbProxyView()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var vm = DataContext as ICustomListViewModel;
            if (vm != null)
            {
                vm.InitializeMenus();
            }
            SubscribeSource();
        }

        private void SubscribeSource()
        {
            var vm = DataContext as IModelHandler;
            if (vm == null)
                return;

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
    }
}
