using System.Windows;
using Microsoft.Practices.Unity;
using wmsMLC.General.Modules;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.General.PL.WPF
{
    public sealed class Bootstraper
    {
        private IUnityContainer _container;

        private void ConfigureContainer()
        {
            // добавлем глобальный регистратор модулей
            _container.RegisterInstance<IModuleRegistrator>(new ModuleRegistrator(), new ContainerControlledLifetimeManager());
        }

        public void Run()
        {
            var moduleCatalog = new ConfigurationModuleCatalog();
            moduleCatalog.Initialize();

            _container = new UnityContainer();

            ConfigureContainer();

            //Загружаем модули
            using (var manager = new ModuleManager(new ModuleInitializer(_container), moduleCatalog))
                manager.Run();

            CreateShell();
        }

        public void RunModules()
        {
            var mr = _container.Resolve<IModuleRegistrator>();
            var modules = mr.GetModules();
            foreach (var m in modules)
            {
                m.Run();
            }
        }

        private void CreateShell()
        {
            // т.к. Shell у нас лежит совсем в другом модуле, а модули еще могут быть не прогружены, делаем предпроверку
            var shell = (_container.IsRegistered<IShell>() ? _container.Resolve<IShell>() : null) as Window;

            if (shell == null)
                throw new DeveloperException("Can't create shell. Check modules loading.");

            if (Application.Current.MainWindow == null)
                Application.Current.MainWindow = shell;
        }
    }
}