using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectTreeView))]
    public class BillAnalyticsTreeViewModel : ObjectTreeViewModelBase<BillAnalytics>
    {
        public BillAnalyticsTreeViewModel()
        {
            KeyPropertyName = BillAnalytics.AnalitycsCodePropertyName.ToUpper();
            ParentIdPropertyName = BillAnalytics.AnalitycsCodeRPropertyName.ToUpper();
        }
    }
}