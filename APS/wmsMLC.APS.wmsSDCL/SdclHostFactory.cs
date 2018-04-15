using System.ServiceProcess;
using wmsMLC.General.Services.Service;

namespace wmsMLC.APS.wmsSDCL
{
    internal class SdclHostFactory : IHostFactory
    {
        public ServiceBase GetService(ServiceContext context)
        {
            return new SdclHost(context);
        }

        public IAppHost GetApp(ServiceContext context)
        {
            return new SdclHost(context);
        }
    }
}
