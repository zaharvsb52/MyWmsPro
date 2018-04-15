using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using wmsMLC.DCL.Chat.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.DCL.Chat.Views
{
    /// <summary>
    /// Interaction logic for ConversationView.xaml
    /// </summary>
    public partial class ConversationView : DXPanelView
    {

        #region .  BrowserBehavior  .

        public static readonly DependencyProperty HtmlProperty = DependencyProperty.RegisterAttached(
                    "Html",
                    typeof(string),
                    typeof(ConversationView),
                    new FrameworkPropertyMetadata(OnHtmlChanged));

        [AttachedPropertyBrowsableForType(typeof(WebBrowser))]
        public static string GetHtml(WebBrowser d)
        {
            return (string)d.GetValue(HtmlProperty);
        }

        public static void SetHtml(WebBrowser d, string value)
        {
            d.SetValue(HtmlProperty, value);
        }

        static void OnHtmlChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var webBrowser = dependencyObject as WebBrowser;
            if (webBrowser != null)
                webBrowser.NavigateToString(e.NewValue as string ?? "&nbsp;");
        }

        #endregion

        public ConversationView()
        {
            InitializeComponent();
        }

        private void TextEdit_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter)
                return;
            var mv = DataContext as ConversationViewModel;
            if (mv != null)
                mv.OnMessage(messageTextEdit.Text);
            messageTextEdit.Text = string.Empty;
        }

        private void WebBrowser_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            //INFO: не даем открывать никаких ссылок в чате
            //открываем их внешне
            e.Cancel = e.Uri != null;
            if (e.Uri == null)
                return;
            var startInfo = new ProcessStartInfo
            {
                FileName = e.Uri.ToString()
            };
            Process.Start(startInfo);
        }

        //private void messagesListBox_SelectedIndexChanged(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    DispatcherHelper.Invoke(new Action(() => messagesListBox.ScrollIntoView(messagesListBox.SelectedItem)));
        //}
    }
}
