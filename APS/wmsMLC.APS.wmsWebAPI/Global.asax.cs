using System.Web.Http;
using wmsMLC.APS.wmsWebAPI.Properties;
using wmsMLC.Business;
using wmsMLC.General;

namespace wmsMLC.APS.wmsWebAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            BLHelper.InitBL(dalType: DALType.Oracle);
            var auth = IoC.Instance.Resolve<IAuthenticationProvider>();
            auth.Authenticate(Settings.Default.ServiceUserName, Settings.Default.ServicePassword);

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
