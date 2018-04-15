using System.Collections.ObjectModel;
using System.Linq;

namespace wmsMLC.General.PL.WPF.Components.ViewModels.RclMenu
{
    public class ListMenuItem : CommandMenuItem
    {
        public const string MenuItemsPropertyName = "MenuItems";

        private ObservableCollection<MenuItemBase> _menuItems;


        public ListMenuItem()
        {
            MenuItems = new ObservableCollection<MenuItemBase>();
        }

        public ObservableCollection<MenuItemBase> MenuItems
        {
            get
            {
                var ordered = _menuItems.OrderBy(i => i.Priority).ToArray();
                _menuItems.Clear();
                foreach (var item in ordered)
                    _menuItems.Add(item);
                return _menuItems;
            }
            set
            {
                if (_menuItems==value)
                    return;

                _menuItems = value;
                OnPropertyChanged(MenuItemsPropertyName);
            }
        }
    }
}