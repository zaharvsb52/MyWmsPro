using System.Windows;
using System.Windows.Input;
using DevExpress.Xpf.LayoutControl;
using wmsMLC.General.PL.WPF.Components.Helpers;

namespace wmsMLC.General.PL.WPF.Components.Controls.Rcl
{
    public class CustomTile : Tile
    {
        public CustomTile()
        {
            DefaultStyleKey = typeof(CustomTile);
            ShowHotKey = true;
            HotKey = HotKey2 = Key.None;
        }

        #region . Properties .
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(CustomTile));

        public string HotKeyTitle
        {
            get { return (string)GetValue(HotKeyTitleProperty); }
            set { SetValue(HotKeyTitleProperty, value); }
        }
        public static readonly DependencyProperty HotKeyTitleProperty = DependencyProperty.Register("HotKeyTitle", typeof(string), typeof(CustomTile));

        public bool ShowHotKey { get; set; }

        private Key _hotKey;
        public Key HotKey
        {
            get { return _hotKey; }
            set { _hotKey = value; 
                if (ShowHotKey)
                {
                    var hotkey = _hotKey == Key.None ? HotKey2 : _hotKey;
                    if (hotkey == Key.None)
                        return;
                    var numkey = KeyHelper.GetNumKey(hotkey);
                    HotKeyTitle = numkey >= 0 ? string.Format("{0}", numkey) : hotkey.ToString();
                }
            }
        }

        public Key HotKey2 { get; set; }
        #endregion . Properties .

        public void PreviewHotKey(KeyEventArgs e)
        {
            if (HotKey == e.Key || HotKey2 == e.Key)
            {
                OnClick();
                e.Handled = true;
            }
        }
    }
}