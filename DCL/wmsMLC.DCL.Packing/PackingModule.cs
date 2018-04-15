using Microsoft.Practices.Unity;
using wmsMLC.Business.Managers;
using wmsMLC.DCL.Packing.ViewModels;
using wmsMLC.General.PL.WPF;

namespace wmsMLC.DCL.Packing
{
    public sealed class PackingModule : ModuleBase
    {
        public PackingModule(IUnityContainer container) : base(container) { }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            Container.RegisterType<IPackingViewModel, PackingViewModel>();

            Container.RegisterType<IPitInViewModel, PutInViewModel>();

            PackWorkflowCodes.RegisterCachableWorkflow();
        }

        public override void Run()
        {
            base.Run();

            // для отладки
//            var vs = Container.Resolve<IViewService>();
//            var vm = new PackingViewModel();
//            vs.Show(vm);
        }
    }

    public static class PackWorkflowCodes
    {
        public const string WfPack = "WFPACK";

        public static string[] GetCachableWorkflows()
        {
            return new[] {WfPack};
        }

        public static void ClearCachableWorkflow()
        {
            // отмечаем какие workflow можно кэшировать
            foreach (var wf in GetCachableWorkflows())
                BPWorkflowManager.ClearObjectCache(wf);
        }

        internal static void RegisterCachableWorkflow()
        {
            // отмечаем какие workflow можно кэшировать
            foreach (var wf in GetCachableWorkflows())
                BPWorkflowManager.SetObjectCachable(wf);
        }
    }

    public static class PackProcessCodes
    {
        public const string Pack = "PACK";
    }
}