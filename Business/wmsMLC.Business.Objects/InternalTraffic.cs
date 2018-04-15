using System;

namespace wmsMLC.Business.Objects
{
    public class InternalTraffic : WMSBusinessObject
    {
        #region . Constants .

        public const string ExternalTrafficIDPropertyName = "EXTERNALTRAFFICID_R";
        public const string WarehouseCodePropertyName = "WAREHOUSECODE_R";
        public const string GateCodePropertyName = "GATECODE_R";
        public const string InternalTrafficFactArrivedPropertyName = "INTERNALTRAFFICFACTARRIVED";
        public const string InternalTrafficFactDepartedPropertyName = "INTERNALTRAFFICFACTDEPARTED";
        public const string InternalTrafficOrderPropertyName = "INTERNALTRAFFICORDER";
        public const string MandantIDPropertyName = "MANDANTID";
        public const string PURPOSEVISITID_RPropertyName = "PURPOSEVISITID_R";
        public const string STATUSCODE_RPropertyName = "STATUSCODE_R";

        //public const string InternalTrafficPurposePropertyName = "INTERNALTRAFFICPURPOSE";

        #endregion . Constants .

        #region .  Properties  .

        public decimal? MandantID
        {
            get { return GetProperty<decimal?>(MandantIDPropertyName); }
            set { SetProperty(MandantIDPropertyName, value); }
        }

        public decimal? ExternalTrafficID
        {
            get { return GetProperty<decimal?>(ExternalTrafficIDPropertyName); }
            set { SetProperty(ExternalTrafficIDPropertyName, value); }
        }

        public string WarehouseCode
        {
            get { return GetProperty<string>(WarehouseCodePropertyName); }
            set { SetProperty(WarehouseCodePropertyName, value); }
        }

        public string GateCode
        {
            get { return GetProperty<string>(GateCodePropertyName); }
            set { SetProperty(GateCodePropertyName, value); }
        }

        public DateTime? TrafficFactArrived
        {
            get { return GetProperty<DateTime?>(InternalTrafficFactArrivedPropertyName); }
            set { SetProperty(InternalTrafficFactArrivedPropertyName, value); }
        }

        public DateTime? TrafficFactDeparted
        {
            get { return GetProperty<DateTime?>(InternalTrafficFactDepartedPropertyName); }
            set { SetProperty(InternalTrafficFactDepartedPropertyName, value); }
        }

        public decimal? PURPOSEVISITID_R
        {
            get { return GetProperty<decimal?>(PURPOSEVISITID_RPropertyName); }
            set { SetProperty(PURPOSEVISITID_RPropertyName, value); }
        }

        public string STATUSCODE_R
        {
            get { return GetProperty<string>(STATUSCODE_RPropertyName); }
            set { SetProperty(STATUSCODE_RPropertyName, value); }
        }

        public InternalTrafficStatus Status
        {
            get
            {
                var status = GetProperty<string>(STATUSCODE_RPropertyName);
                return (InternalTrafficStatus)Enum.Parse(typeof(InternalTrafficStatus), status);
            }
            set { SetProperty(STATUSCODE_RPropertyName, value.ToString()); }
        }
        #endregion .  Properties  .
    }
}