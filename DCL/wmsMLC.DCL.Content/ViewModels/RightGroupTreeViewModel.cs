using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof (ObjectTreeView))]
    public class RightGroupTreeViewModel : ObjectTreeViewModelBase<RightGroup>
    {
        public RightGroupTreeViewModel()
        {
            KeyPropertyName = new RightGroup().GetPrimaryKeyPropertyName();
            ParentIdPropertyName = RightGroup.RightGroupParentPropertyName.ToUpper();
        }
    }
}