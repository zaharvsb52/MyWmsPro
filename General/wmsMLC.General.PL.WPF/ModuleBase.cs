using System;
using System.Windows;
using Microsoft.Practices.Unity;
using wmsMLC.General.Modules;

namespace wmsMLC.General.PL.WPF
{
    public abstract class ModuleBase : IRunableModule
    {
        public const string ViewServiceRegisterSuffixListShow = "List.Show";
        public const string ViewServiceRegisterSuffixTreeShow = "Tree.Show";

        protected IUnityContainer Container { get; private set; }

        protected ModuleBase(IUnityContainer container)
        {
            Container = container;

            // регистриуем себя для последующей обработки
            container.Resolve<IModuleRegistrator>().Register(this);
        }

        public void Initialize()
        {
            ConfigureContainer();
        }

        protected virtual void ConfigureContainer() { }

        public virtual void Run() { }

        protected static void LoadApplicationResource(string url)
        {
            Application.Current.Resources.MergedDictionaries.Add((ResourceDictionary)Application.LoadComponent(new Uri(url, UriKind.Relative)));
        }
    }
}