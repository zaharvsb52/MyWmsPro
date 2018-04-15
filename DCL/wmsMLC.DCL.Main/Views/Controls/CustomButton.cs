using System.Windows.Controls;
using System.Windows.Input;

namespace wmsMLC.DCL.Main.Views.Controls
{
    public class CustomButton : Button
    {

        #region .  Properties  .

        public Key HotKey { get; set; }

        #endregion

        public CustomButton()
        {
            HotKey = Key.None;
        }

        public bool IsHotKey(Key key)
        {
            return HotKey == key;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            PreviewHotKey(e);
            base.OnPreviewKeyDown(e);
        }

        public bool PreviewHotKey(KeyEventArgs e)
        {
            if (!IsHotKey(e.Key)) return e.Handled;
            Focus();
            OnClick();
            e.Handled = true;
            return e.Handled;
        }
    }
}