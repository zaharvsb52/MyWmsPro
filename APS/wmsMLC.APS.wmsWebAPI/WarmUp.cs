using System.Web.Hosting;
using log4net;
using wmsMLC.EPS.wmsEPS.Helpers;
using wmsMLC.General;

namespace wmsMLC.APS.wmsWebAPI
{
    public class WarmUp : IProcessHostPreloadClient 
    {
        public void Preload(string[] parameters)
        {
            Log4NetHelper.Configure("WebApi");
            var log = LogManager.GetLogger(GetType());
            EpsHelper.Initialize();
            log.Info("WarmUp complete");
        }
    }
}