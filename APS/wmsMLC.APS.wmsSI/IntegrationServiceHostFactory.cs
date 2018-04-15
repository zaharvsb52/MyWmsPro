using System.ServiceProcess;
using wmsMLC.General.Services.Service;

namespace wmsMLC.APS.wmsSI
{
    public class IntegrationServiceHostFactory : IHostFactory
    {
        public ServiceBase GetService(ServiceContext context)
        {
            return new IntegrationServiceHost(context);
        }

        public IAppHost GetApp(ServiceContext context)
        {
            return new IntegrationServiceHost(context);
        }
    }
}
