using wmsMLC.General;

namespace wmsMLC.Business.Objects
{
    public class OutputTask : WMSBusinessObject
    {
        #region .  Constants  .
        public const string OutputTaskCodePropertyName = "OUTPUTTASKCODE";
        public const string OutputTaskFeedbackPropertyName = "OUTPUTTASKFEEDBACK";
        public const string OutputTaskTimePropertyName = "OUTPUTTASKTIME";
        public const string OutputTaskStatusPropertyName = "OUTPUTTASKSTATUS";
        public const string OutputTaskOrderPropertyName = "OUTPUTTASKORDER";
        public const string TaskParamsPropertyName = "TASKPARAMS";
        #endregion .  Constants  .

        #region .  Properties  .
        public string OutputTaskCode
        {
            get { return GetProperty<string>(OutputTaskCodePropertyName); }
            set { SetProperty(OutputTaskCodePropertyName, value); }
        }

        public string OutputTaskFeedback
        {
            get { return GetProperty<string>(OutputTaskFeedbackPropertyName); }
            set { SetProperty(OutputTaskFeedbackPropertyName, value); }
        }

        public string OutputTaskTime
        {
            get { return GetProperty<string>(OutputTaskTimePropertyName); }
            set { SetProperty(OutputTaskTimePropertyName, value); }
        }

        public string OutputTaskStatus
        {
            get { return GetProperty<string>(OutputTaskStatusPropertyName); }
            set { SetProperty(OutputTaskStatusPropertyName, value); }
        }

        public decimal OutputTaskOrder
        {
            get { return GetProperty<decimal>(OutputTaskOrderPropertyName); }
            set { SetProperty(OutputTaskOrderPropertyName, value); }
        }

        public WMSBusinessCollection<OutputParam> TaskParams
        {
            get { return GetProperty<WMSBusinessCollection<OutputParam>>(TaskParamsPropertyName); }
            set { SetProperty(TaskParamsPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}