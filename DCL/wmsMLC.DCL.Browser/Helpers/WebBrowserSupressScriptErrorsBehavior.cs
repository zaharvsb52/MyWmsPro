using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using DevExpress.Mvvm.UI.Interactivity;

namespace wmsMLC.DCL.Browser.Helpers
{
    public class WebBrowserSupressScriptErrorsBehavior : Behavior<WebBrowser>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Navigated += OnWebBrowserNavigated;
        }
        protected override void OnDetaching()
        {
            AssociatedObject.Navigated -= OnWebBrowserNavigated;
            base.OnDetaching();
        }
        void OnWebBrowserNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            SetSilent((WebBrowser)AssociatedObject, true);
        }

        static Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
        static Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");
        static void SetSilent(WebBrowser browser, bool silent)
        {
            return;
            IOleServiceProvider provider = browser.Document as IOleServiceProvider;
            if (provider == null) return;
            object webBrowser;
            provider.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
            if (webBrowser == null) return;
            Type webBrowserType = webBrowser.GetType();
            webBrowserType.InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
        }
        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }
    }

    public class Browser2Helper
    {
        static Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
        static Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");
        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }

        static dynamic GetBrowser(WebBrowser browser)
        {
            var provider = browser.Document as IOleServiceProvider;
            if (provider == null)
                return null;

            object webBrowser;
            provider.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
            if (webBrowser == null)
                return null;

            return webBrowser;
        }
    }

}