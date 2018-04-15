using System.Linq;

namespace wmsMLC.DCL.General.ViewModels.Menu
{
    public class ListMenuItem : CommandMenuItem
    {
        public const string MenuItemsPropertyName = "MenuItems";
        public const string IsEnableItemsPropertyName = "IsEnableItems";

        private MenuItemCollection _menuItems;
        private bool _isEnableItems;

        public ListMenuItem()
        {
            MenuItems = new MenuItemCollection();
            IsEnableItems = false;
        }

        private string _name;
        public override string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                MenuItems.ParentName = _name;
            }
        }

        public MenuItemCollection MenuItems
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
                if (_menuItems == value)
                    return;

                _menuItems = value;
                OnPropertyChanged(MenuItemsPropertyName);
            }
        }

        public bool IsEnableItems
        {
            get { return _isEnableItems; }
            set
            {
                if (_isEnableItems == value)
                    return;

                _isEnableItems = value;
                OnPropertyChanged(IsEnableItemsPropertyName);
            }
        }
    }
}