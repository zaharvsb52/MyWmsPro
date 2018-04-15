using System;
using System.Configuration;
using wmsMLC.DCL.Browser.Views;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Browser.ViewModels
{
    [View(typeof (BrowserView))]
    public class CustomsViewModel : BrowserViewModel
    {
        private const string CustomsControllerCommand =
            "#command=runControllerCstCustoms&controller=MLC.wms.cstReqCustoms.ReqCustomsController";

        public CustomsViewModel()
        {
            PanelCaption = "Таможня";
            var baseUri = new Uri(ConfigurationManager.AppSettings["WebclientUrl"]);
            Url = new Uri(baseUri, CustomsControllerCommand);
        }
    }
}