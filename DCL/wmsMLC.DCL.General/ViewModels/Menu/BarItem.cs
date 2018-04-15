using System.Collections.ObjectModel;
using System.Linq;

namespace wmsMLC.DCL.General.ViewModels.Menu
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
        private MenuItemCollection _menuItems;
        #endregion

        public BarItem()
        {
            MenuItems = new MenuItemCollection();
            IsVisible = true;
            IsEnable = true;
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

        private bool _allowHide;
        /// <summary>
        /// Свойство управляет возможностью делать панель инструментов видимой/невидимой.
        /// </summary>
        public bool AllowHide
        {
            get { return _allowHide; }
            set
            {
                if (_allowHide == value)
                    return;
                OnPropertyChanged("AllowHide");
            }
        }

        public MenuItemCollection MenuItems
        {
            get
            {
                //var ordered = _menuItems.OrderBy(i => i.Priority).ToArray();
                //_menuItems.Clear();
                //foreach (var item in ordered)
                //{
                //    _menuItems.Add(item);
                //}
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

    public class MenuItemCollection : ObservableCollection<MenuItemBase>
    {
        public string ParentName { get; set; }

        protected override void InsertItem(int index, MenuItemBase item)
        {
            if (this.Count != 0)
            {
                var prior = this.FirstOrDefault(i => i.Priority >= item.Priority);
                if (prior != null)
                    index = this.IndexOf(prior);
            }
            base.InsertItem(index, item);

            if (!string.IsNullOrEmpty(ParentName) && string.IsNullOrEmpty(item.Name))
                CreateName(item);
        }

        private void CreateName(MenuItemBase item)
        {
            item.Name = string.Format("Menu{0}{1:000}", ParentName, IndexOf(item) + 1);
        }
    }
}