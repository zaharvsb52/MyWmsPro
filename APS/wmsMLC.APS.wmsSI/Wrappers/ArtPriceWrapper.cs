namespace wmsMLC.APS.wmsSI.Wrappers
{
    public class ArtPriceWrapper : BaseWrapper
    {
        #region . Properties .

        public System.DateTime? ARTPRICEDATEBEGIN { get; set; }
        public System.DateTime? ARTPRICEDATEEND { get; set; }
        public System.Decimal? ARTPRICEID { get; set; }
        public System.Double? ARTPRICEVALUE { get; set; }
        public System.Double? ARTPRICEVAT { get; set; }
        public System.Decimal? MANDANTID { get; set; }
        public System.Decimal? SKUID_R { get; set; }

        public string MandantCode { get; set; }

        #endregion
    }
}