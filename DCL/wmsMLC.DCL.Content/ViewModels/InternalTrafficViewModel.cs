using System.Collections.ObjectModel;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof (ObjectView))]
    public class InternalTrafficViewModel : ObjectViewModelBase<InternalTraffic>
    {
        protected override void SourceObjectPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.SourceObjectPropertyChanged(sender, e);

            if (Source == null) 
                return;

            var editable = Source as IEditable;
            if (editable.IsInRejectChanges)
                return;

            if (Source.IsNew)
            {
                if (!e.PropertyName.EqIgnoreCase(InternalTraffic.ExternalTrafficIDPropertyName))
                    return;

                OnPropertyChanged("Source");

                var mgr = (IInternalTrafficManager) GetManager();
                if (mgr != null)
                    mgr.FillMandant(Source);
            }
            else
            {
                if (!e.PropertyName.EqIgnoreCase(InternalTraffic.WarehouseCodePropertyName))
                    return;

                OnPropertyChanged("Source");

                if (Source.WarehouseCode == null)
                {
                    Source.GateCode = null;
                }
            }
        }

        protected override ObservableCollection<DataField> GetFields(SettingDisplay displaySetting)
        {
            var fields = base.GetFields(displaySetting);
            foreach (var f in fields)
            {
                if (f.Name != InternalTraffic.ExternalTrafficIDPropertyName)
                    continue;
                f.IsChangeLookupCode = true;
                f.LookupFilterExt = Source.IsNew ? @"STATUSCODE_R='CAR_ARRIVED'" : null;
                break;
            }
            return fields;
        }
    }
}