namespace wmsMLC.Business.Objects
{
    public class BPBatch : WMSBusinessObject
    {
        public const string WorkflowCodePropertyName = "WORKFLOWCODE_R";
        public const string BatchNamePropertyName = "BATCHNAME";

        public string WorkflowCode
        {
            get { return GetProperty<string>(WorkflowCodePropertyName); }
            set { SetProperty(WorkflowCodePropertyName, value); }
        }

        public string BatchName
        {
            get { return GetProperty<string>(BatchNamePropertyName); }
            set { SetProperty(BatchNamePropertyName, value); }
        }
    }
}