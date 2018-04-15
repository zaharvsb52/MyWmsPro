using Microsoft.Practices.Unity;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.WorkflowDesigner.Compilers;
using wmsMLC.DCL.WorkflowDesigner.ViewModels;
using wmsMLC.DCL.WorkflowDesigner.ViewModels.ActivityLoader;
using wmsMLC.DCL.WorkflowDesigner.ViewModels.Designer;
using wmsMLC.DCL.WorkflowDesigner.ViewModels.Toolbox;
using wmsMLC.General.PL.WPF;

namespace wmsMLC.DCL.WorkflowDesigner
{
    public sealed class WorkflowDesignerModule : ModuleBase
    {
        public WorkflowDesignerModule(IUnityContainer container) : base(container) { }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            Container.RegisterType<IWDUCLViewModel, WDUCLViewModel<BPWorkflow>>();
            Container.RegisterType<IDesignerViewModel, DesignerViewModel>();
            Container.RegisterType<IActivityLoader, CompiledActivityLoader>();
            Container.RegisterType<IToolboxCreatorService, ToolboxCreatorServiceWithCustomActivities>();
            Container.RegisterType<IToolboxViewModel, StandardToolboxViewModel>();
            Container.RegisterType<IActivityCompiler, ActivityCompiler>();
            Container.RegisterType<IXamlViewModel, IXamlViewModel>();
        }
    }
}
