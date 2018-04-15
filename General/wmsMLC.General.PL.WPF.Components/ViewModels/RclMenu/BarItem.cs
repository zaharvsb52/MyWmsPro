using System.Collections.ObjectModel;
using System.Linq;

namespace wmsMLC.General.PL.WPF.Components.ViewModels.RclMenu
{
    public class BarItem : MenuViewModelBase
    {
        #region .  Fields && Consts .
        public const string CaptionPropertyName = "Caption";
        public const string MenuItemsPropertyName = "MenuItems";
        public const string IsVisiblePropertyName = "IsVisible";
        public const string GlyphSizePropertyName = "GlyphSize";

        private GlyphSizeType _glyphSize;
        private bool _isVisible;
        private string _caption;
        private ObservableCollection<MenuItemBase> _menuItems;
        #endregion

        public BarItem()
        {
            MenuItems = new ObservableCollection<MenuItemBase>();
            IsVisible = true;
        }

        #region .  Properties  .
        public string Caption
        {
            get { return _caption; }
            set
            {
                if (_caption == value)
                    return;

                _caption = value;
                OnPropertyChanged(CaptionPropertyName);
            }
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
                if (_menuItems == value)
                    return;

                _menuItems = value;
                OnPropertyChanged(MenuItemsPropertyName);
            }
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible == value)
                    return;

                _isVisible = value;
                OnPropertyChanged(IsVisiblePropertyName);
            }
        }

        public GlyphSizeType GlyphSize
        {
            get { return _glyphSize; }
            set
            {
                if (_glyphSize == value)
                    return;

                _glyphSize = value;
                OnPropertyChanged(GlyphSizePropertyName);
            }
        }
        #endregion .  Properties  .
    }

    public enum GlyphSizeType
    {
        Default = 0,
        Small = 1,
        Large = 2,
    }
}