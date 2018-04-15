using System;
using System.Windows;
using Microsoft.Practices.Unity;
using wmsMLC.DCL.Configurator.ViewModels;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.General.PL.WPF;

namespace wmsMLC.DCL.Configurator
{
    public class ConfiguratorModule : ModuleBase
    {
        public ConfiguratorModule(IUnityContainer container) : base(container) { }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            Container.RegisterType<IPmConfigViewModel, PmConfigViewModel>();
        }

        public override void Run()
        {
            base.Run();

            Application.Current.Resources.MergedDictionaries.Add((ResourceDictionary)Application.LoadComponent(new Uri("/wmsMLC.DCL.Configurator;Component/Themes/Generic.xaml", UriKind.Relative)));
        }
    }
}
