using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectListView))]
    public class EventDetailListViewModel : ObjectListViewModelBase<EventDetail>
    {
        protected override bool CanEdit()
        {
            // Детализация событий не редактируется
            return false;
        }

        protected override bool CanShowHistory()
        {
            // Детализация событий не имеет истории
            return false;
        }
    }
}