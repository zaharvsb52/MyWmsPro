using System;
using System.Collections.Generic;

namespace wmsMLC.APS.wmsSI.Wrappers
{
    public class YExternalTrafficWrapper : BaseWrapper
    {
        public DateTime? EXTERNALTRAFFICPLANARRIVED { get; set; }
        public DateTime? EXTERNALTRAFFICFACTDEPARTED { get; set; }
        public DateTime? EXTERNALTRAFFICFACTARRIVED { get; set; }
        public Decimal? VEHICLEID_R { get; set; }
        public Decimal? PARKINGID_R { get; set; }
        public Decimal? MANDANTID { get; set; }
        public Decimal? EXTERNALTRAFFICID { get; set; }
        public Decimal? EXTERNALTRAFFICFORVARDER { get; set; }
        public Decimal? EXTERNALTRAFFICDRIVER { get; set; }
        public Decimal? EXTERNALTRAFFICCARRIER { get; set; }
        public String VVEHICLETYPE { get; set; }
        public String VVEHICLERN { get; set; }
        public String STATUSCODE_R { get; set; }
        public String EXTERNALTRAFFICTRAILERRN { get; set; }
        public String EXTERNALTRAFFICPASSNUMBER { get; set; }
        public String EXTERNALTRAFFICHOSTREF { get; set; }        

        public string CarrierName { get; set; }
        public string MandantCode { get; set; }
        public VehicleWrapper Vehicle { get; set; }
        public WorkerWrapper Driver { get; set; }
        public ParkingWrapper Parking { get; set; }

        public List<CargoIWBWrapper> CargoIWBList { get; set; }
        public List<CargoOWBWrapper> CargoOWBList { get; set; }
        public List<InternalTrafficWrapper> InternalTrafficList { get; set; }

        //Не используется в DataContract'е
        public decimal? WORKERPASSID_R { get; set; }
    }
}
