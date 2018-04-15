using System.ServiceProcess;
using wmsMLC.General.Services.Service;
namespace wmsMLC.APS.wmsEPS
{
    public class EpsHostFactory : IHostFactory
    {
        public ServiceBase GetService(ServiceContext context)
        {
            return new EPSService(context);
        }

        public IAppHost GetApp(ServiceContext context)
        {
            return new EPSService(context);
        }
    }
}
