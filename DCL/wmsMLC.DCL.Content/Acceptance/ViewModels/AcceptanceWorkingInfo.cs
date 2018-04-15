using wmsMLC.Business.Objects;
using wmsMLC.General.BL;

namespace wmsMLC.DCL.Content.Acceptance.ViewModels
{
    [SysObjectName("Working")]
    public class AcceptanceWorkingInfo : Working
    {
        public const string OperationPropertyName = "Operation";

        public AcceptanceWorkingInfo() { }

        public AcceptanceWorkingInfo(Working working, string operation)
        {
            if (working == null)
                return;

            try
            {
                SuspendNotifications();
                Copy(working, this);
                Operation = operation;
                AcceptChanges();
            }
            finally
            {
                ResumeNotifications();
            }
        }

        public string Operation { get; set; }
    }
}