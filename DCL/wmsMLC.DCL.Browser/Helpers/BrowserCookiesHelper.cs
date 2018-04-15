using System;
using System.Configuration;
using System.Net.Http;
using System.Reflection;
using System.Windows;
using log4net;
using MLC.SvcClient;
using wmsMLC.General;

namespace wmsMLC.DCL.Browser.Helpers
{
    public static class BrowserCookiesHelper
    {
        private const string AuthCookieName = ".WmsAuth";

        static readonly ILog Log = LogManager.GetLogger(typeof(BrowserCookiesHelper));
        static readonly FieldInfo HandlerField = typeof(HttpMessageInvoker).GetField("handler",
            BindingFlags.Instance | BindingFlags.NonPublic);

        public static void AuthenticateBrowser()
        {
            // читаем из настроек url подключения к webapp
            var url = ConfigurationManager.AppSettings["WebclientUrl"];
            if (url == null)
                throw new InvalidOperationException(
                    "Не задан обязательный параметр 'WebclientUrl'. Пожалуйста, проверьте конфигурационный файл.");
            var uri = new Uri(url);

            if (IsCookieExists(uri))
                return;

            // получаем клиента в котором проходила аутентификация
            var httpClientStore = IoC.Instance.Resolve<IHttpClientStore>();
            var client = httpClientStore.GetOrCreate(url);

            // HACK: у клиента ну ни как нельзя получить handler - потому лезем, через reflection
            var handler = (HttpClientHandler)HandlerField.GetValue(client);
            var cookies = handler.CookieContainer.GetCookies(uri);

            // получаем аутентификационную куку
            var authCookie = cookies[AuthCookieName];
            if (authCookie == null)
            {
                Log.WarnFormat("Не удалось найти аутентификационную куку в httpClient.");
                return;
            }

            //if (cookie != null)
            //    cookie += "; " + authCookie;
            //else
            //     cookie = authCookie.ToString();

            // выставляем куку
            Application.SetCookie(uri, authCookie.ToString());
        }

        public static bool IsCookieExists(Uri uri)
        {
            try
            {
                //получаем куки для приложения и проверяем наличие аутентификации
                var cookie = Application.GetCookie(uri);
                if (cookie != null && cookie.Contains(AuthCookieName))
                    return true;
            }
            catch (Exception ex)
            {
                Log.Warn("Не удалось проверить наличие аутентификационной куки. " + ex.Message, ex);
            }
            return false;
        }
    }
}