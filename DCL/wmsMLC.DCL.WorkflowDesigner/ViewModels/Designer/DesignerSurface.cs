using System.Activities.Presentation.Validation;
using System.Collections.Generic;

namespace wmsMLC.DCL.WorkflowDesigner.ViewModels.Designer
{
    public class DesignerSurface : IDesignerSurface
    {
        private readonly CacheValidationErrorService _cves;

        public DesignerSurface(System.Activities.Presentation.WorkflowDesigner designer)
        {
            this.Designer = designer;
            this._cves = new CacheValidationErrorService();
            this.Designer.Context.Services.Publish<IValidationErrorService>(this._cves);
        }

        public IList<ValidationErrorInfo> GetLastErrors()
        {
            return this._cves.Errors;
        }

        public System.Activities.Presentation.WorkflowDesigner Designer { get; set; }
    }
}
