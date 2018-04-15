using System.Collections.ObjectModel;

namespace wmsMLC.DCL.General.ViewModels.Menu
{
    public class EditMenuItem : CommandMenuItem
    {
        public ObservableCollection<string> ComboBoxItems { get; set; }
        public string EditValue { get; set; }
    }
}