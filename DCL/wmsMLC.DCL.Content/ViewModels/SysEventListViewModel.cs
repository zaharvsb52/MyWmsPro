using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectListView))]
    public class SysEventListViewModel : ObjectListViewModelBase<SysEvent>
    {
        public SysEventListViewModel()
        {
            ShowDetail = true;
        }

        protected override bool CanNew()
        {
            return false;
        }

        protected override bool CanDelete()
        {
            return false;
        }

        protected override bool CanShowHistory()
        {
            return false;
        }

    }
}