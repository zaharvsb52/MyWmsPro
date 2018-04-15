using System.Collections.Generic;

namespace wmsMLC.DCL.WorkflowDesigner
{
    public interface IWDUCLViewModelInternal
    {
        void PostStatus(string message);
        void On_DisplayErrors(IEnumerable<DesignerErrorDefinition> errors);
        void LoadSource();
        IDesignerViewModel GetDesignerViewModel();
        IXamlViewModel GetXamlViewModel();
        void StartWait();
        void StopWait();
    }
}
