using Microsoft.Practices.Unity;
using wmsMLC.DCL.Main.ViewModels;
using wmsMLC.DCL.Topology.ViewModels;
using wmsMLC.General.PL.WPF;

namespace wmsMLC.DCL.Topology
{
    public sealed class TopologyModule : ModuleBase
    {
        public TopologyModule(IUnityContainer container) : base(container) { }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            Container.RegisterType<ITopologyViewModel, TopologyViewModel>();
        }
    }
}