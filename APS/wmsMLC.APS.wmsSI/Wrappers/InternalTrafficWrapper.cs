namespace wmsMLC.APS.wmsSI.Wrappers
{
    public class InternalTrafficWrapper : BaseWrapper
    {
        #region . Properites .

        public System.Decimal? EXTERNALTRAFFICID_R { get; set; }
        public System.String GATECODE_R { get; set; }
        public System.DateTime? INTERNALTRAFFICFACTARRIVED { get; set; }
        public System.DateTime? INTERNALTRAFFICFACTDEPARTED { get; set; }
        public System.Decimal? INTERNALTRAFFICID { get; set; }
        public System.Decimal? INTERNALTRAFFICORDER { get; set; }
        public System.String INTERNALTRAFFICPURPOSE { get; set; }
        public System.Decimal? MANDANTID { get; set; }
        public System.String WAREHOUSECODE_R { get; set; }

        public string MandantCode { get; set; }
        #endregion
    }
}
