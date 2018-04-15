using System.Activities.Presentation.Validation;
using System.Collections.Generic;

namespace wmsMLC.DCL.WorkflowDesigner
{
    public interface IDesignerSurface
    {
        IList<ValidationErrorInfo> GetLastErrors();
        System.Activities.Presentation.WorkflowDesigner Designer { get; }
    }
}
