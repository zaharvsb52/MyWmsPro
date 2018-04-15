using System.Windows.Input;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.ViewModels;
using wmsMLC.General.PL.WPF.Views;
#pragma warning disable 1998

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectView))]
    public class DashboardViewModel : ObjectViewModelBase<Dashboard>
    {
        private bool _initializing;

        public DashboardViewModel()
        {
            _initializing = true;

            ShowDesignerCommand = new DelegateCustomCommand(ShowDesigner, CanShowDesigner);
        }

        public override void CreateProcessMenu()
        {
            base.CreateProcessMenu();
            var bar = Menu.GetOrCreateBarItem(StringResources.BusinessProcesses, 1, "BarItemBusinessProcesses");
            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.WDUCL,
                Command = ShowDesignerCommand,
                HotKey = new KeyGesture(Key.F12),
                ImageSmall = ImageResources.DCLDashboardDesigner16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLDashboardDesigner32.GetBitmapImage(),
                DisplayMode = DisplayModeType.Default,
                //GlyphSize = GlyphSizeType.Large,
                GlyphAlignment = GlyphAlignmentType.Top,
                Priority = 1000
            });
        }

        public ICustomCommand ShowDesignerCommand { get; private set; }

        protected override void OnSourceChanged()
        {
            base.OnSourceChanged();
            // при инициализации объекта прочтем его xaml из БД
            if (_initializing)
            {
                _initializing = false;
                var mgr = IoC.Instance.Resolve<IXamlManager<Dashboard>>();
                var key = Source.GetKey();
                string xaml = null;
                if (key != null)
                    xaml = mgr.GetXaml(key.ToString());

                if (!string.IsNullOrEmpty(xaml))
                {
                    try
                    {
                        Source.SuspendNotifications();
                        Source.SuspendValidating();
                        Source.SetProperty(Dashboard.DashboardBodyPropertyName, xaml);
                        var eo = Source as IEditable;
                        if (eo != null)
                            eo.AcceptChanges(Dashboard.DashboardBodyPropertyName);
                    }
                    finally
                    {
                        Source.ResumeNotifications();
                        Source.ResumeValidating();
                    }
                }
            }
        }

        protected override void RefreshData(bool usewait)
        {
            try
            {
                _initializing = true;
                base.RefreshData(usewait);
            }
            finally
            {
                //Если нет прав на Refresh
                _initializing = false;
            }
        }

        private bool CanShowDesigner()
        {
            var ed = Source as IEditable;
            if (ed == null || ed.IsDirty)
                return false;
            var isNew = Source as IIsNew;
            if (isNew == null || isNew.IsNew)
                return false;
            return true;
        }

        protected override void OnIsDirtyChanged(IEditable eo)
        {
            base.OnIsDirtyChanged(eo);
            ShowDesignerCommand.RaiseCanExecuteChanged();
        }

        private async void ShowDesigner()
        {
            try
            {
                WaitStart();
                //Обновляем WF перед дизайном
                RefreshData(false);
                var dvm = IoC.Instance.Resolve<IDashboardDesignerViewModel>();
                // если не реализован дизайнер, то и не покажем
                if (dvm == null)
                    throw new DeveloperException("Not registered.");
                var source = (WMSBusinessObject)Source.Clone();
                source.AcceptChanges();
                dvm.SetSource(source);
                var vs = IoC.Instance.Resolve<IViewService>();
                vs.Show((IViewModel)dvm, new ShowContext { DockingType = DockType.Document });
            }
            finally
            {
                WaitStop();
            }
        }

        protected override bool Save()
        {
            // запомним значение xaml иначе оно будет стерто при сохраннии
            var xaml = Source.GetProperty(Dashboard.DashboardBodyPropertyName);
            if (xaml != null)
                Source.SetProperty(Dashboard.DashboardBodyPropertyName, null);

            // Если не сохранили запись, нет смысла сохранять XAML
            if (!base.Save())
            {
                Source.SetProperty(Dashboard.DashboardBodyPropertyName, xaml);
                return false;
            }

            var result = true;
            try
            {
                WaitStart();
                var mgr = IoC.Instance.Resolve<IXamlManager<Dashboard>>();
                Source.SuspendNotifications();
                Source.SuspendValidating();
                Source.SetProperty(Dashboard.DashboardBodyPropertyName, xaml);
                mgr.SetXaml(Source.GetKey().ToString(), xaml != null ? xaml.ToString() : string.Empty);
                var eo = Source as IEditable;
                if (eo != null)
                    eo.AcceptChanges();
            }
            catch
            {
                result = false;
            }
            finally
            {
                WaitStop();
                Source.ResumeNotifications();
                Source.ResumeValidating();
            }
            return result;
        }
    }
}