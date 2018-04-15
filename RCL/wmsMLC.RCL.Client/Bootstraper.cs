using System.Windows;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using wmsMLC.General;
using wmsMLC.RCL.Client.Properties;
using wmsMLC.RCL.General;
using wmsMLC.RCL.General.Helpers;
using wmsMLC.RCL.General.Views;

namespace wmsMLC.RCL.Client
{
    internal class Bootstraper : UnityBootstrapper
    {
        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            // добавлем глобальный регистратор модулей
            Container.RegisterInstance<IModuleRegistrator>(new ModuleRegistrator(), new ContainerControlledLifetimeManager());
        }

        protected override void ConfigureServiceLocator()
        {
            base.ConfigureServiceLocator();

            // устанавливаем контейнер для ServiceLocator
            var provider = new UnityServiceLocator(Container);
            ServiceLocator.SetLocatorProvider(() => provider);
        }

        public override void Run(bool runWithDefaultConfiguration)
        {
            base.Run(runWithDefaultConfiguration);

            // после запуска у нас уже будет зарегистированный IShell
            Shell = CreateShell();
            if (Shell == null)
            {
                //throw new DeveloperException(DeveloperExceptionResources.CantCreateShell);
                throw new DeveloperException("Can't create shell.");
            }

            // запускаем RegionManager
            var mgr = Container.Resolve<IRegionManager>();
            RegionManager.SetRegionManager(Shell, mgr);
            RegionManager.UpdateRegions();

            InitializeShell();
        }

        public void RunModules()
        {
            var mr = Container.Resolve<IModuleRegistrator>();
            var modules = mr.GetModules();
            foreach (var m in modules)
                m.Run();
        }

        protected override DependencyObject CreateShell()
        {
            // т.к. Shell у нас лежит совсем в другом модуле, а модули еще могут быть не прогружены, делаем предпроверку
            if (!Container.IsRegistered<IShell>())
                return null;

            return (DependencyObject)Container.Resolve<IShell>();
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            var mc = new MaskDirectoryModuleCatalog
                {
                    ModulePath = Settings.Default.ModulePath,
                    SearchMask = "wmsMLC.RCL.*.dll"
                };

            // добавлем главный служебный модуль
            mc.AddModule(typeof(Main.Module));
            return mc;
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();
            Application.Current.MainWindow = (Window)Shell;
        }
    }
}