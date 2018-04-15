
using System.Collections.Concurrent;
using MLC.WebClient;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.General;

namespace wmsMLC.Business.DAL.Service
{
    public abstract class BPWorkflowRepository : XamlRepository<BPWorkflow, string> 
    {
        const string Package = "WMS";
        const string Version = "1.0.0.0";
        private readonly WmsAPI _api;
        private static readonly ConcurrentDictionary<string, string> RegisterWf = new ConcurrentDictionary<string, string>();

        protected BPWorkflowRepository()
        {
            if (_api == null)
                _api = IoC.Instance.Resolve<WmsAPI>();
        }

        public override string GetXaml(string pKey)
        {
            return _api.GetWorkflow(Package, pKey, Version);
        }

        public override void SetXaml(string pKey, string xaml)
        {
            _api.SetWorkflow(Package, pKey, Version, xaml);
        }
    }
}
