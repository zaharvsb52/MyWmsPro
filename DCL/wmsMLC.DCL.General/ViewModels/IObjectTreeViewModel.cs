using System.Windows.Input;

namespace wmsMLC.DCL.General.ViewModels
{
    public interface IObjectTreeViewModel
    {
        string KeyPropertyName { get; set; }
        string ParentIdPropertyName { get; set; }
        bool ShowNodeImage { get; set; }
        string DefaultSortingField { get; set; }
        bool AutoExpandAllNodes { get; set; }
        ICommand EditCommand { get; }
        KeyGesture GetEditKey();
    }
}
