using System.Activities.Presentation.Toolbox;
using System.Collections.Generic;

namespace wmsMLC.DCL.WorkflowDesigner
{
    public interface IToolboxCreatorService
    {
        IEnumerable<ToolboxCategory> GetToolboxCategories();
    }
}
