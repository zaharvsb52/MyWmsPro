using System.Windows.Input;

namespace wmsMLC.General.PL.WPF.Components.ViewModels
{
    public interface IRclListViewModel : IRclPanelViewModel, IRclMenuHandler
    {
        object SelectedItem { get; set; }
        ICommand ItemSelectCommand { get; }
    }
}
