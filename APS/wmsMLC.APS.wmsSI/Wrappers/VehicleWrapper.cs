namespace wmsMLC.APS.wmsSI.Wrappers
{
    public class VehicleWrapper : BaseWrapper
    {
        public System.Decimal? VEHICLETRAILERMAXWEIGHT { get; set; }
        public System.Decimal? VEHICLETRAILEREMPTYWEIGHT { get; set; }
        public System.Decimal? VEHICLEPERSON { get; set; }
        public System.Decimal? VEHICLEOWNERLEGAL { get; set; }
        public System.Decimal? VEHICLEMAXWEIGHT { get; set; }
        public System.Decimal? VEHICLEID { get; set; }
        public System.Decimal? VEHICLEEMPTYWEIGHT { get; set; }
        public System.Decimal? CARTYPEID_R { get; set; }
        public System.String VEHICLEVIN { get; set; }
        public System.String VEHICLETYPE { get; set; }
        public System.String VEHICLETRAILERRN { get; set; }
        public System.String VEHICLERN { get; set; }
        public System.String VEHICLEHOSTREF { get; set; }
        public System.String VEHICLECOLOR { get; set; }
        public System.String VEHICLESEMITRAILERRN { get; set; }
        public System.Decimal? VEHICLESEMITRAILERMAXWEIGHT { get; set; }
        public System.Decimal? VEHICLESEMITRAILEREMPTYWEIGHT { get; set; }

        public CarTypeWrapper CARTYPE { get; set; }
        public PartnerWrapper OWNERLEGAL { get; set; }
        public WorkerWrapper OWNERPERSON { get; set; }
    }
}
