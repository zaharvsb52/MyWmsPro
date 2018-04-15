using System;
using System.Collections.Generic;

namespace wmsMLC.Business.Objects
{
    public class ExternalTraffic : WMSBusinessObject
    {
        #region . Constants .

        public const string ExternalTrafficIDPropertyName = "EXTERNALTRAFFICID";
        public const string VehicleIDPropertyName = "VEHICLEID_R";
        public const string ExternalTrafficTrailerRNPropertyName = "EXTERNALTRAFFICTRAILERRN";
        public const string ExternalTrafficPlanArrivedPropertyName = "EXTERNALTRAFFICPLANARRIVED";
        public const string ExternalTrafficFactArrivedPropertyName = "EXTERNALTRAFFICFACTARRIVED";
        public const string ExternalTrafficFactDepartedPropertyName = "EXTERNALTRAFFICFACTDEPARTED";
        public const string StatusCode_RPropertyName = "STATUSCODE_R";
        public const string ExternalTrafficDriverPropertyName = "EXTERNALTRAFFICDRIVER";
        public const string ExternalTrafficForvarderPropertyName = "EXTERNALTRAFFICFORVARDER";
        public const string ExternalTrafficCarrierPropertyName = "EXTERNALTRAFFICCARRIER";
        public const string ParkingIDPropertyName = "PARKINGID_R";
        public const string ExternalTrafficHostRefPropertyName = "EXTERNALTRAFFICHOSTREF";
        public const string MandantIDPropertyName = "MANDANTID";
        public const string EXTERNALTRAFFICPASSNUMBERPropertyName = "EXTERNALTRAFFICPASSNUMBER";
        public const string InternalTrafficListPropertyName = "INTERNALTRAFFICL";
        public const string VVehicleRNPropertyName = "VVEHICLERN";
        public const string WORKERPASSID_RPropertyName = "WORKERPASSID_R";

        #endregion . Constants .

        #region .  Properties  .

        public decimal? VehicleID_R
        {
            get { return GetProperty<decimal?>(VehicleIDPropertyName); }
            set { SetProperty(VehicleIDPropertyName, value); }
        }
        public decimal? MandantID
        {
            get { return GetProperty<decimal?>(MandantIDPropertyName); }
            set { SetProperty(MandantIDPropertyName, value); }
        }
        public IEnumerable<InternalTraffic> InternalTrafficList
        {
            get { return GetProperty<IEnumerable<InternalTraffic>>(InternalTrafficListPropertyName); }
            set { SetProperty(InternalTrafficListPropertyName, value); }
        }

        public decimal? Driver
        {
            get { return GetProperty<decimal?>(ExternalTrafficDriverPropertyName); }
            set { SetProperty(ExternalTrafficDriverPropertyName, value); }
        }

        public string VVehicleRN
        {
            get { return GetProperty<string>(VVehicleRNPropertyName); }
            set { SetProperty(VVehicleRNPropertyName, value); }
        }

        public ExternalTrafficStatus Status
        {
            get
            {
                var status = GetProperty<string>(StatusCode_RPropertyName);
                return (ExternalTrafficStatus) Enum.Parse(typeof (ExternalTrafficStatus), status);
            }
            set { SetProperty(StatusCode_RPropertyName, value.ToString()); }
        }

        public decimal? WORKERPASSID_R
        {
            get { return GetProperty<decimal?>(WORKERPASSID_RPropertyName); }
            set { SetProperty(WORKERPASSID_RPropertyName, value); }
        }

        #endregion .  Properties  .
    }
}