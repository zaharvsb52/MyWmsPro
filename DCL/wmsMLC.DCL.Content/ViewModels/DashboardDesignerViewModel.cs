using System.Globalization;
using System.IO;
using System.Windows.Input;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.Content.Views;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Views;
using System;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(DashboardDesignerView))]
    public class DashboardDesignerViewModel : SourceViewModelBase<Dashboard>, IHaveUniqueName, IDashboardDesignerViewModel
    {
        private string _uniqueName;

        public ICommand SaveDashboardCommand { get; private set; }

        public DashboardDesignerViewModel()
        {
            AllowClosePanel = true;

            SaveDashboardCommand = new DelegateCustomCommand<DevExpress.DashboardCommon.Dashboard>(OnSaveDashboard, CanSaveDashboard);
        }

        public void SetSource(object source)
        {
            var src = (Dashboard) source;
            Source = src;
            var key = src.GetKey();
            _uniqueName = key != null ? key.ToString() : string.Empty;
            PanelCaption = GetUniqueName();
        }

        protected override void OnSourceChanged()
        {
            base.OnSourceChanged();
            PanelCaption = GetUniqueName();
        }

        public string GetUniqueName()
        {
            return _uniqueName;
        }

        protected bool CanSaveDashboard(DevExpress.DashboardCommon.Dashboard dashboard)
        {
            return true;
        }

        protected void OnSaveDashboard(DevExpress.DashboardCommon.Dashboard dashboard)
        {
            try
            {
                WaitStart();
                var xaml = GetDashboardXml(dashboard);
                var xamlManager = IoC.Instance.Resolve<IXamlManager<Dashboard>>();
                var mgr = IoC.Instance.Resolve<IBaseManager<Dashboard>>();
                Source.SuspendNotifications();
                Source.SuspendValidating();
                Source.SetProperty(Dashboard.DashboardBodyPropertyName, xaml);
                IncrementVersion(Source);
                xamlManager.SetXaml(Source.GetKey().ToString(), xaml ?? string.Empty);
                mgr.Update(Source);
                mgr.ClearCache();
                Source = mgr.Get(Source.GetKey());
                var eo = Source as IEditable;
                if (eo != null)
                    eo.AcceptChanges();
            }
            finally
            {
                WaitStop();
                Source.ResumeNotifications();
                Source.ResumeValidating();
            }
        }

        private static void IncrementVersion(Dashboard dashboard)
        {
            var oldVersion = dashboard.GetProperty<string>(Dashboard.DashboardVersionPropertyName);
            if (!String.IsNullOrEmpty(oldVersion))
            {
                var ar = oldVersion.Split('.');
                int newVersion;
                if (Int32.TryParse(ar[ar.Length - 1], out newVersion))
                {
                    ar[ar.Length - 1] = (++newVersion).ToString(CultureInfo.InvariantCulture);
                    dashboard.SetProperty(Dashboard.DashboardVersionPropertyName, String.Join(".", ar));
                }
            }
        }

        private static string GetDashboardXml(DevExpress.DashboardCommon.Dashboard dashboard)
        {
            string xaml;
            using (var stream = new MemoryStream())
            using (var sr = new StreamReader(stream))
            {
                dashboard.SaveToXml(stream);
                stream.Seek(0, SeekOrigin.Begin);
                xaml = sr.ReadToEnd();
            }
            return xaml;
        }
    }
}