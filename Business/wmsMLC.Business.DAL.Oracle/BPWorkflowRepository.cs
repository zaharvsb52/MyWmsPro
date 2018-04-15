using wmsMLC.Business.Objects.Processes;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class BPWorkflowRepository : XamlRepository<BPWorkflow, string>
    {
        protected BPWorkflowRepository()
        {
            PkgName = "pkgBpWorkflow";
            GetName = "getBpWorkflowXAML";
            UpdName = "updBpWorkflowXAML";
        }
    }
}
