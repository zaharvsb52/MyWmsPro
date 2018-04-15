using System;

namespace wmsMLC.Business.Objects
{
    public class EventHeader : WMSBusinessObject
    {
        #region .  Constants  .
        public const string StartTimePropertyName = "EVENTHEADERSTARTTIME";
        public const string EndTimePropertyName = "EVENTHEADERENDTIME";
        public const string InstancePropertyName = "EVENTHEADERINSTANCE";
        public const string EventKindCodePropertyName = "EVENTKINDCODE_R";
        public const string OperationCodePropertyName = "OPERATIONCODE_R";
        public const string MandantIDPropertyName = "MANDANTID";
        public const string ProcessCodePropertyName = "PROCESSCODE_R";
        public const string StatusCodePropertyName = "STATUSCODE_R";
        #endregion .  Constants  .

        #region .  Properties  .
        public DateTime StartTime
        {
            get { return GetProperty<DateTime>(StartTimePropertyName); }
            set { SetProperty(StartTimePropertyName, value); }
        }

        public DateTime? EndTime
        {
            get { return GetProperty<DateTime>(EndTimePropertyName); }
            set { SetProperty(EndTimePropertyName, value); }
        }

        public string Instance
        {
            get { return GetProperty<string>(InstancePropertyName); }
            set { SetProperty(InstancePropertyName, value); }
        }

        public string EventKindCode
        {
            get { return GetProperty<string>(EventKindCodePropertyName); }
            set { SetProperty(EventKindCodePropertyName, value); }
        }

        public string OperationCode
        {
            get { return GetProperty<string>(OperationCodePropertyName); }
            set { SetProperty(OperationCodePropertyName, value); }
        }

        public decimal? MandantID
        {
            get { return GetProperty<decimal?>(MandantIDPropertyName); }
            set { SetProperty(MandantIDPropertyName, value); }
        }

        public string ProcessCode
        {
            get { return GetProperty<string>(ProcessCodePropertyName); }
            set { SetProperty(ProcessCodePropertyName, value); }
        }

        public decimal? StatusCode
        {
            get { return GetProperty<decimal?>(StatusCodePropertyName); }
            set { SetProperty(StatusCodePropertyName, value); }
        }
        #endregion .  Properties  .
    }
}
