using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Utils;
using wmsMLC.General;

namespace wmsMLC.DCL.Main.Views.Controls
{
    /// <summary>
    /// Контрол исполюзуется для настройки тулбаров (форма "Настройки", закладка "Параметры").
    /// </summary>
    public partial class CustomOptionsControl
    {
        public CustomOptionsControl()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                var item = GetBarButtonItemLink();
                if (item != null)
                {
                    displayMode.EditValue = item.BarItemDisplayMode;
                    captionGlyphAlignment.EditValue = item.UserGlyphAlignment ?? Dock.Top;
                }
                else
                {
                    displayMode.SelectedIndex = 0;
                    captionGlyphAlignment.SelectedIndex = 1;
                }
            };

            ceLargeIconsInToolbars.Loaded += OnCeLargeIconsInToolbarsLoaded;
            ceLargeIconsInToolbars.EditValueChanged += OnCeLargeIconsInToolbarsEditValueChanged;
            ceLargeIconsInMenu.Loaded += OnCeLargeIconsInMenuLoaded;
            ceLargeIconsInMenu.EditValueChanged += OnCeLargeIconsInMenuEditValueChanged;
            ceShowScreenTipsOnToolbars.Loaded += OnCeShowScreenTipsOnToolbarsLoaded;
            ceShowScreenTipsOnToolbars.EditValueChanged += OnCeShowScreenTipsOnToolbarsEditValueChanged;
            ceShowShortcutKeysOnScreenTips.Loaded += OnCeShowShortcutKeysOnScreenTipsLoaded;
            ceShowShortcutKeysOnScreenTips.EditValueChanged += OnCeShowShortcutKeysOnScreenTipsEditValueChanged;
        }

        public CustomBarManager Manager
        {
            get
            {
                return (CustomBarManager)GetValue(ManagerProperty);
            }
            set
            {
                SetValue(ManagerProperty, value);
            }
        }

        public static readonly DependencyProperty ManagerProperty = DependencyPropertyManager.Register("Manager", typeof(CustomBarManager), typeof(CustomOptionsControl));

        private void OnCeLargeIconsInToolbarsLoaded(object sender, RoutedEventArgs e)
        {
            if (Manager == null) 
                return;
            ((CheckEdit)sender).IsChecked = Manager.ToolbarGlyphSize == GlyphSize.Default ? (bool?) null : Manager.ToolbarGlyphSize == GlyphSize.Large;
        }

        private void OnCeLargeIconsInToolbarsEditValueChanged(object sender, EditValueChangedEventArgs e)
        {
            if (Manager == null) 
                return;
            if (!((CheckEdit)sender).IsChecked.HasValue)
            {
                Manager.ToolbarGlyphSize = GlyphSize.Default;
            }
            else
            {
                Manager.ToolbarGlyphSize = ((CheckEdit)sender).IsChecked == true ? GlyphSize.Large : GlyphSize.Small;
            }
        }

        private void OnCeLargeIconsInMenuLoaded(object sender, RoutedEventArgs e)
        {
            if (Manager == null)
                return;
            ((CheckEdit)sender).IsChecked = Manager.MenuGlyphSize == GlyphSize.Default ? (bool?)null : Manager.MenuGlyphSize == GlyphSize.Large;
        }

        private void OnCeLargeIconsInMenuEditValueChanged(object sender, EditValueChangedEventArgs e)
        {
            if (Manager == null)
                return;
            if (!((CheckEdit)sender).IsChecked.HasValue)
            {
                Manager.MenuGlyphSize = GlyphSize.Default;
            }
            else
            {
                Manager.MenuGlyphSize = ((CheckEdit)sender).IsChecked == true ? GlyphSize.Large : GlyphSize.Small;
            }
        }

        private void OnCeShowScreenTipsOnToolbarsEditValueChanged(object sender, EditValueChangedEventArgs e)
        {
            if (Manager != null)
            {
                Manager.ShowScreenTips = ((CheckEdit)sender).IsChecked == true;
                OnCeShowShortcutKeysOnScreenTipsLoaded(ceShowShortcutKeysOnScreenTips, null);
            }
        }

       private void OnCeShowScreenTipsOnToolbarsLoaded(object sender, RoutedEventArgs e)
        {
            if (Manager != null)
            {
                ((CheckEdit)sender).IsChecked = Manager.ShowScreenTips;
            }
        }

       private void OnCeShowShortcutKeysOnScreenTipsEditValueChanged(object sender, EditValueChangedEventArgs e)
        {
            if (Manager != null)
            {
                Manager.ShowShortcutInScreenTips = ((CheckEdit)sender).IsChecked == true;
            }
        }

       private void OnCeShowShortcutKeysOnScreenTipsLoaded(object sender, RoutedEventArgs e)
        {
            if (Manager != null)
            {
                ((CheckEdit)sender).IsEnabled = ceShowScreenTipsOnToolbars.IsChecked == true;
                ((CheckEdit)sender).IsChecked = Manager.ShowShortcutInScreenTips;
            }
        }

        private void displayMode_SelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            if (Manager == null || displayMode.SelectedItem == null)
                return;

            var itemDisplayMode = displayMode.SelectedItem.To(BarItemDisplayMode.Default);
            Manager.BarItemDisplayMode = itemDisplayMode;
            captionGlyphAlignment.IsEnabled = itemDisplayMode == BarItemDisplayMode.ContentAndGlyph;

            foreach (var item in Manager.Bars.SelectMany(bar => bar.ItemLinks.OfType<BarItemLink>()))
            {
                item.BarItemDisplayMode = itemDisplayMode;
            }
        }

        private void captionGlyphAlignment_SelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            if (Manager == null || captionGlyphAlignment.SelectedItem == null)
                return;

            var glyphAlignment = captionGlyphAlignment.SelectedItem.To(Dock.Top);
            Manager.UserGlyphAlignment = glyphAlignment;

            foreach (var item in Manager.Bars.SelectMany(bar => bar.ItemLinks.OfType<BarItemLink>()))
            {
                item.UserGlyphAlignment = glyphAlignment;
            }
        }

        private BarItemLink GetBarButtonItemLink()
        {
            if (Manager == null || Manager.Bars.Count == 0 || Manager.Bars[0].ItemLinks.Count == 0)
                return null;
            return Manager.Bars.SelectMany(bar => bar.ItemLinks).OfType<BarItemLink>().FirstOrDefault();
        }
    }
}
