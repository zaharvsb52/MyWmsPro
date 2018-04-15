using wmsMLC.Business.Objects;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General.PL.WPF.ViewModels;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectListView))]
    public class BillCalcConfigListViewModel : XAMLListViewModelBase<BillCalcConfig>
    {
        protected override IViewModel WrappModelIntoVM(BillCalcConfig model)
        {
            return new BillCalcConfigViewModel { Source = model };
        }
    }
}