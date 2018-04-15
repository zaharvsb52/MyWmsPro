using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectTreeView))]
    public class UIButtonTreeViewModel : ObjectTreeViewModelBase<UIButton>
    {
        public UIButtonTreeViewModel()
        {
            KeyPropertyName = UIButton.UIButtonCodePropertyName.ToUpper();
            ParentIdPropertyName = UIButton.UIButtonParentPropertyName.ToUpper();
        }
    }
}