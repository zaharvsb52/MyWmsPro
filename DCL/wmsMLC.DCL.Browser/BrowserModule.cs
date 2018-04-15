using System;
using log4net;
using Microsoft.Practices.Unity;
using wmsMLC.DCL.Browser.ViewModels;
using wmsMLC.DCL.General;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.DCL.Browser
{
    public sealed class BrowserModule : ModuleBase
    {
        private readonly ILog _log = LogManager.GetLogger(typeof (BrowserModule));

        public BrowserModule(IUnityContainer container) : base(container)
        {
        }

        public override void Run()
        {
            // модуль впринципе не сможет работать на WindowXP
            if (!IsWinVistaOrHigher())
                return;

            try
            {
                base.Run();

                var viewService = Container.Resolve<IViewService>();
                viewService.Register(DclModules.Customs, typeof (CustomsViewModel));

                _log.Info("Модуль Browser успешно запущен");
            }
            catch (Exception ex)
            {
                _log.Error("Не удалось проинициализировать модуль таможни", ex);
            }
        }

        private static bool IsWinVistaOrHigher()
        {
            var os = Environment.OSVersion;
            return (os.Platform == PlatformID.Win32NT) && (os.Version.Major >= 6);
        }
    }
}