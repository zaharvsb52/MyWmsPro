using System.Collections.ObjectModel;

namespace wmsMLC.General.PL.WPF.Components.ViewModels.RclMenu
{
    public class EditMenuItem : CommandMenuItem
    {
        public ObservableCollection<string> ComboBoxItems { get; set; }
        public string EditValue { get; set; }
    }
}