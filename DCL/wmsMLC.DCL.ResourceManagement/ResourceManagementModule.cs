using Microsoft.Practices.Unity;
using wmsMLC.DCL.Main.ViewModels;
using wmsMLC.DCL.ResourceManagement.ViewModels;
using wmsMLC.General.PL.WPF;

namespace wmsMLC.DCL.ResourceManagement
{
    public sealed class ResourceManagementModule : ModuleBase
    {
        public ResourceManagementModule(IUnityContainer container) : base(container) { }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            Container.RegisterType<IWorkingManageViewModel, WorkingManageViewModel>();
        }
    }
}