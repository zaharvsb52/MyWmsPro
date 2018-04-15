namespace wmsMLC.APS.wmsSI.Wrappers
{
    public class CargoIWBPosWrapper : BaseWrapper
    {
        #region . Properites .

        public System.Decimal? CARGOIWBID_R { get; set; }        
        public System.Decimal? CARGOIWBPOSCOUNT { get; set; }
        public System.String CARGOIWBPOSDESC { get; set; }
        public System.Decimal? CARGOIWBPOSID { get; set; }
        public System.String CARGOIWBPOSTYPE { get; set; }
        public System.Decimal? IWBID_R { get; set; }
        public QLF QLFCODE_R { get; set; }
        public System.String TETYPECODE_R { get; set; }

        #endregion
    }

    public enum QLF
    {
        QLFNORMAL, QLFQUALITY, QLFDEFECT
    }
}
