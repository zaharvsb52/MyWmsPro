using wmsMLC.Business.Objects;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General.PL.WPF.ViewModels;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectListView))]
    public class DashboardListViewModel : XAMLListViewModelBase<Dashboard>
    {
        protected override IViewModel WrappModelIntoVM(Dashboard model)
        {
            return new DashboardViewModel { Source = model };
        }
    }
}