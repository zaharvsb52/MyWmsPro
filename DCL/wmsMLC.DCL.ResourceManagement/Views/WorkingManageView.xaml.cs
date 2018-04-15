using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Scheduler;
using DevExpress.XtraScheduler;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.ResourceManagement.ViewModels;
using wmsMLC.General;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.DCL.ResourceManagement.Views
{
    public partial class WorkingManageView : DXPanelView, IHelpHandler
    {
        public WorkingManageView()
        {
            InitializeComponent();

            scheduler.Views.TimelineView.ClearValue(SchedulerViewBase.HorizontalAppointmentStyleSelectorProperty);
            scheduler.GroupType = SchedulerGroupType.Resource;

            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var vm = DataContext as WorkingManageViewModel;
            if (vm == null)
                return;

            scheduler.Storage.AppointmentStorage.Labels.Clear();
            foreach (var label in vm.Labels)
            {
                var appointmentLabel = new AppointmentLabel(label.Color, label.Name, label.Name);
                scheduler.Storage.AppointmentStorage.Labels.Add(appointmentLabel);
            }
        }

        private void SchedulerStorage_FilterAppointment(object sender, PersistentObjectCancelEventArgs e)
        {
            var dx = DataContext as WorkingManageViewModel;
            if (dx == null)
                return;

            var a = (Appointment)e.Object;
            var obj = a.GetSourceObject(scheduler.GetCoreStorage()) as AppointmentModel;
            var ops = cbOperation.SelectedItems.Cast<OperationModel>().ToArray();
            var res = cbResources.SelectedItems.Cast<ResourceModel>().ToArray();
            var showCompleted = ceShowCompleted.IsChecked.HasValue && ceShowCompleted.IsChecked.Value;
            e.Cancel = !dx.IsAppointmentVisible(obj, ops, res, showCompleted);
        }

        private void OnEditValueChanged(object sender, EditValueChangedEventArgs e)
        {
            scheduler.ActiveView.LayoutChanged();
        }

        private void SchedulerStorage_OnFilterResource(object sender, PersistentObjectCancelEventArgs e)
        {
            e.Cancel = true;
            if (cbResources.SelectedItems.Count == 0)
                return;

            var res = (Resource)e.Object;
            var obj = res.GetSourceObject(scheduler.GetCoreStorage()) as ResourceModel;
            if (obj == null)
                return;
            e.Cancel = cbResources.SelectedItems.Cast<ResourceModel>().All(i => i.Id != obj.Id);
        }

        private void OnEditAppointmentFormShowing(object sender, EditAppointmentFormEventArgs e)
        {
            e.Cancel = true;
            var ovm = IoC.Instance.Resolve<IObjectViewModel<Working>>();
            var obj = (AppointmentModel)e.Appointment.GetSourceObject(scheduler.GetCoreStorage());
            ovm.SetSource(obj.Working);

            var vs = IoC.Instance.Resolve<IViewService>();
            vs.ShowDialogWindow(ovm, true);
        }

        private void OnAppointmentViewInfoCustomizing(object sender, AppointmentViewInfoCustomizingEventArgs e)
        {
            var obj = (AppointmentModel)e.ViewInfo.Appointment.GetSourceObject(scheduler.GetCoreStorage());
            e.ViewInfo.HasRightBorder = obj.IsCompleted;
            e.ViewInfo.IsEndVisible = obj.IsCompleted;

            e.ViewInfo.HasLeftBorder = true;
        }

        private void Scheduler_OnAllowAppointmentDelete(object sender, AppointmentOperationEventArgs e)
        {
            e.Allow = false;
        }

        private void Scheduler_OnAllowInplaceEditor(object sender, AppointmentOperationEventArgs e)
        {
            e.Allow = false;
        }

        private void Scheduler_OnAllowAppointmentCreate(object sender, AppointmentOperationEventArgs e)
        {
            e.Allow = false;
        }

        private void Scheduler_OnAllowAppointmentDrag(object sender, AppointmentOperationEventArgs e)
        {
            e.Allow = false;
        }

        private void Scheduler_OnAllowAppointmentResize(object sender, AppointmentOperationEventArgs e)
        {
            e.Allow = false;
        }

        #region . IHelpHandler .
        string IHelpHandler.GetHelpLink()
        {
            return null;
        }

        string IHelpHandler.GetHelpEntity()
        {
            return "WorkingManage";
        }
        #endregion
    }

    public class AppointmentSnapToCellsModeList : List<AppointmentSnapToCellsMode>
    {
    }
    public class AppointmentStatusDisplayTypeList : List<AppointmentStatusDisplayType>
    {
    }

}
