using System.ServiceProcess;

namespace wmsMLC.General.Services.Service
{
    /// <summary>
    /// Фабрика создания приложений. Умеет создавать сервисы и консоли
    /// </summary>
    public interface IHostFactory
    {
        ServiceBase GetService(ServiceContext context);
        IAppHost GetApp(ServiceContext context);
    }
}
