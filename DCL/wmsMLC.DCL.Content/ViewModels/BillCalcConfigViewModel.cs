using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectView))]
    public class BillCalcConfigViewModel : ObjectViewModelBase<BillCalcConfig>
    {
        private bool _initializing;

        public BillCalcConfigViewModel()
        {
            _initializing = true;
        }
        
        protected override void OnSourceChanged()
        {
            base.OnSourceChanged();
            // при инициализации объекта прочтем его xaml из БД
            if (_initializing)
            {
                _initializing = false;
                var mgr = IoC.Instance.Resolve<IXamlManager<BillCalcConfig>>();
                var key = Source.GetKey();
                string xaml = null;
                if (key != null)
                    xaml = mgr.GetXaml(key);

                if (!string.IsNullOrEmpty(xaml))
                {
                    try
                    {
                        Source.SuspendNotifications();
                        Source.SuspendValidating();
                        Source.SetProperty(BillCalcConfig.SQLPropertyName, xaml);
                        var eo = Source as IEditable;
                        if (eo != null)
                            eo.AcceptChanges(BillCalcConfig.SQLPropertyName);
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
                _initializing = false;
            }
        }

        public override async void RefreshDataAsync()
        {
            //base.RefreshDataAsync();
            RefreshData();
        }

        protected override bool Save()
        {
            var sql = Source.GetProperty(BillCalcConfig.SQLPropertyName);
            if (sql != null)
                Source.SetProperty(BillCalcConfig.SQLPropertyName, null);
            
            if (!base.Save())
            {
                Source.SetProperty(BillCalcConfig.SQLPropertyName, sql);
                return false;
            }

            var result = true;
            try
            {
                WaitStart();
                Source.SuspendNotifications();
                Source.SuspendValidating();
                Source.SetProperty(BillCalcConfig.SQLPropertyName, sql);
                var mgr = IoC.Instance.Resolve<IXamlManager<BillCalcConfig>>();
                mgr.SetXaml(Source.GetKey(), sql != null ? sql.ToString() : string.Empty);
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