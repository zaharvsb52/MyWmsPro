using System.Windows.Input;
using DevExpress.Xpf.Bars;
using wmsMLC.DCL.Browser.Helpers;
using wmsMLC.DCL.Main.Views;

namespace wmsMLC.DCL.Browser.Views
{
    public partial class BrowserView : DXPanelView
    {
        public BrowserView()
        {
            InitializeComponent();

            // Одной аутентификации в модуле почему-то не достаточно (после закрытия вкладки снова просит аутентификацию).
            // Потому делаю еще и здесь, т.е. на каждое открытие.
            BrowserCookiesHelper.AuthenticateBrowser();
        }

        public override bool CanClose()
        {
            // NOTE: Тут проблема.
            //       После диспоуза прилетает событие о смене фокуса, и объект, при попытке его обработать падает,
            Keyboard.ClearFocus();
            return true;
        }

        private void BarItem_OnItemClick(object sender, ItemClickEventArgs e)
        {
            tBrowser.Refresh();
        }
    }
}