using System.Activities.Presentation.Validation;
using System.Collections.Generic;

namespace wmsMLC.DCL.WorkflowDesigner.ViewModels.Designer
{
    public class CacheValidationErrorService : IValidationErrorService
    {
        public CacheValidationErrorService()
        {
            this.Errors = new List<ValidationErrorInfo>();
        }

        public void ShowValidationErrors(IList<ValidationErrorInfo> errors)
        {
            this.Errors = errors;
        }

        public IList<ValidationErrorInfo> Errors { get; private set; }
    }
}
