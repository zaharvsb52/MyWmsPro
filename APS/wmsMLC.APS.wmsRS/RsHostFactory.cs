using System.ServiceProcess;
using wmsMLC.General.Services.Service;

namespace wmsMLC.APS.wmsRS
{
    internal class RsHostFactory : IHostFactory
    {
        public ServiceBase GetService(ServiceContext context)
        {
            return new RsHost(context);
        }

        public IAppHost GetApp(ServiceContext context)
        {
            return new RsHost(context);
        }
    }
}