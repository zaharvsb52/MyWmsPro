using Microsoft.Practices.Unity;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Components.ViewModels;
using wmsMLC.General.PL.WPF.Factory;
using wmsMLC.RCL.Content.Factory;
using wmsMLC.RCL.Content.ViewModels;

namespace wmsMLC.RCL.Content
{
    public sealed class ContentModule : ModuleBase
    {
        //private readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(ContentModule));
        public ContentModule(IUnityContainer container) : base(container) {}

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            RegisterFactory();

            RegisterModels();
        }

        private void RegisterModels()
        {
            Container.RegisterType(typeof(IDialogSourceViewModel<TEType>), typeof(TETypeListViewModel));
        }

        private void RegisterFactory()
        {
            IoC.Instance.Register(typeof(IObjectListFactory), typeof(RclObjectListFactory));
        }
    }
}
