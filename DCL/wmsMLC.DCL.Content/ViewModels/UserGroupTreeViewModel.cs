using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectTreeView))]
    public class UserGroupTreeViewModel : ObjectTreeViewModelBase<UserGroup>
    {
        public UserGroupTreeViewModel()
        {
            KeyPropertyName = UserGroup.UserGroupCodePropertyName.ToUpper();
            ParentIdPropertyName = UserGroup.UserGroupParentPropertyName.ToUpper();
        }
    }
}
