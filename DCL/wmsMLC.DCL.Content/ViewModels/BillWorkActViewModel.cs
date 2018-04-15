using System.Collections.ObjectModel;
using System.Linq;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectView))]
    public class BillWorkActViewModel : ObjectViewModelBase<BillWorkAct>
    {

        protected override ObservableCollection<DataField> GetFields(SettingDisplay displaySetting)
        {
            var fields = base.GetFields(displaySetting);

            var fieldValue =
                fields.SingleOrDefault(p => Extensions.EqIgnoreCase(p.Name, BillWorkAct.WORKACT2OP2CLPropertyName));
            if (fieldValue == null)
                return fields;

            if (Source != null && Source.STATUSCODE_R == BillWorkActStatus.WORKACT_COMPLETED.ToString())
                fieldValue.IsEnabled = false;

            return fields;
        }
    }
}