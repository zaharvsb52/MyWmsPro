using wmsMLC.Business.Objects.Processes;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General.PL.WPF.ViewModels;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectListView))]
    public class BPWorkflowListViewModel : XAMLListViewModelBase<BPWorkflow>
    {
        protected override IViewModel WrappModelIntoVM(BPWorkflow model)
        {
            return new BPWorkflowViewModel { Source = model };
        }
    }
}
