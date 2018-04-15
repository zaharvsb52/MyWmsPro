using System;
using System.Collections;
using System.Windows;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.Main.ViewModels;

namespace wmsMLC.DCL.Main.Views
{
    public partial class DashboardView : DXPanelView, IHelpHandler
    {
        public DashboardView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;

            dash.Update();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            SubscribeSource();
        }

        private void SubscribeSource()
        {
            var vm = DataContext as DashboardViewModel;
            if (vm == null)
                return;
            
            vm.ExportDashboard -= vm_ExportDashboard;
            vm.RefreshDashboard -= vm_RefreshDashboard;

            vm.ExportDashboard += vm_ExportDashboard;
            vm.RefreshDashboard += vm_RefreshDashboard;
        }

        private void UnSubscribeSource()
        {
            var vm = DataContext as DashboardViewModel;
            if (vm == null)
                return;

            vm.ExportDashboard -= vm_ExportDashboard;
            vm.RefreshDashboard -= vm_RefreshDashboard;
        }

        void vm_RefreshDashboard(object sender, EventArgs e)
        {
            var vm = DataContext as DashboardViewModel;
            if (vm == null)
                return;
            SourceChanged(vm.ListDataSource);
        }

        void vm_ExportDashboard(object sender, EventArgs e)
        {
            dash.ShowPrintPreview();
        }

        private void SourceChanged(IList ds)
        {
            for (var i = 0; i < ds.Count; i++)
            {
                if (ds[i] != null)
                    dash.Dashboard.DataSources[i].Data = ds[i];
            }
        }

        protected override void Dispose(bool disposing)
        {
            UnSubscribeSource();
            base.Dispose(disposing);
        }

        #region . IHelpHandler .
        string IHelpHandler.GetHelpLink()
        {
            return null;
        }

        string IHelpHandler.GetHelpEntity()
        {
            return "Dashboard";
        }
        #endregion
    }
}


