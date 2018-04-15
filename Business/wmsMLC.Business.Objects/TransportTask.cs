using System;

namespace wmsMLC.Business.Objects
{
    public class TransportTask : WMSBusinessObject
    {
        #region .  Constants  .
        public const string StatusCodePropertyName = "STATUSCODE_R";
        public const string TECodePropertyName = "TECODE_R";
        public const string TaskTypeCodePropertyName = "TTASKTYPECODE_R";
        public const string TaskStartPlaceCodePropertyName = "TTASKSTARTPLACE";
        public const string TaskCurrentPlacePropertyName = "TTASKCURRENTPLACE";
        public const string TaskNextPlacePropertyName = "TTASKNEXTPLACE";
        public const string TaskFinishPlacePropertyName = "TTASKFINISHPLACE";
        public const string ClientCodePropertyName = "CLIENTCODE_R";
        public const string TaskPriorityPropertyName = "TTASKPRIORITY";
        public const string TruckCode_RPropertyName = "TRUCKCODE_R";
        public const string TtaskBeginPropertyName = "TTASKBEGIN";
        public const string TtaskIDPropertyName = "TTASKID";
        public const string StrategyPropertyName = "TRANSPORTTASKSTRATEGY";
        public const string TargetTEPropertyName = "TTASKTARGETTE";
        #endregion .  Constants  .

        #region .  Properties  .
        public TTaskStates StatusCode
        {
            get
            {
                var status = GetProperty<string>(StatusCodePropertyName);
                return (TTaskStates)Enum.Parse(typeof(TTaskStates), status);
            }
            set { SetProperty(StatusCodePropertyName, value.ToString()); }
        }

        public string TECode
        {
            get { return GetProperty<string>(TECodePropertyName); }
            set { SetProperty(TECodePropertyName, value); }
        }

        public string TaskCurrentPlace
        {
            get { return GetProperty<string>(TaskCurrentPlacePropertyName); }
            set { SetProperty(TaskCurrentPlacePropertyName, value); }
        }

        public string TaskNextPlace
        {
            get { return GetProperty<string>(TaskNextPlacePropertyName); }
            set { SetProperty(TaskNextPlacePropertyName, value); }
        }

        public string ClientCode
        {
            get { return GetProperty<string>(ClientCodePropertyName); }
            set { SetProperty(ClientCodePropertyName, value); }
        }

        public string TaskStartPlaceCode
        {
            get { return GetProperty<string>(TaskStartPlaceCodePropertyName); }
            set { SetProperty(TaskStartPlaceCodePropertyName, value); }
        }

        public string TaskFinishPlace
        {
            get { return GetProperty<string>(TaskFinishPlacePropertyName); }
            set { SetProperty(TaskFinishPlacePropertyName, value); }
        }

        public decimal TaskPriority
        {
            get { return GetProperty<decimal>(TaskPriorityPropertyName); }
            set { SetProperty(TaskPriorityPropertyName, value); }
        }

        public string TruckCode_R
        {
            get { return GetProperty<string>(TruckCode_RPropertyName); }
            set { SetProperty(TruckCode_RPropertyName, value); }
        }

        public DateTime? TtaskBegin
        {
            get { return GetProperty<DateTime?>(TtaskBeginPropertyName); }
            set { SetProperty(TtaskBeginPropertyName, value); }
        }

        public string TargetTE
        {
            get { return GetProperty<string>(TargetTEPropertyName); }
            set { SetProperty(TargetTEPropertyName, value); }
        }

        public string Strategy
        {
            get { return GetProperty<string>(StrategyPropertyName); }
            set { SetProperty(StrategyPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}