namespace wmsMLC.Business.Objects
{
    public class EpsTask2Job : WMSBusinessObject
    {
        #region .  Constants  .
        public const string Task2JobOrderPropertyName = "TASK2JOBORDER";
        public const string EpsTask2JobTaskCodePropertyName = "EPSTASK2JOBTASKCODE";

        #endregion .  Constants  .

        #region .  Properties  .
        public decimal Task2JobOrder
        {
            get { return GetProperty<decimal>(Task2JobOrderPropertyName); }
            set { SetProperty(Task2JobOrderPropertyName, value); }
        }

        public string EpsTask2JobTaskCode
        {
            get { return GetProperty<string>(EpsTask2JobTaskCodePropertyName); }
            set { SetProperty(EpsTask2JobTaskCodePropertyName, value); }
        }
        #endregion .  Properties  .
    }
}