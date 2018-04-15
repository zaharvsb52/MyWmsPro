using System.Collections.Generic;

namespace wmsMLC.APS.wmsSI.Wrappers
{
    public class SKUWrapper : BaseWrapper
    {
        #region . Properties .
        public System.Boolean SKUPRIMARY { get; set; }
        public System.Boolean SKUCLIENT { get; set; }
        public System.Decimal? SKUWIDTHCL { get; set; }
        public System.Decimal? SKUWIDTH { get; set; }
        public System.Decimal? SKUWEIGHTCL { get; set; }
        public System.Decimal? SKUWEIGHT { get; set; }
        public System.Decimal? SKULENGTHCL { get; set; }
        public System.Decimal? SKULENGTH { get; set; }
        public System.Decimal? SKUID { get; set; }
        public System.Decimal? SKUHEIGHTCL { get; set; }
        public System.Decimal? SKUHEIGHT { get; set; }
        public System.Decimal? SKUCOUNT { get; set; }
        public System.Decimal? MANDANTID { get; set; }
        public System.String SKURESERVTYPE { get; set; }
        public System.String SKUNAME { get; set; }
        public System.String SKUDESC { get; set; }
        public System.String MEASURECODE_R { get; set; }
        public System.String ARTCODE_R { get; set; }
        public System.Decimal? SKUPARENT { get; set; }
        public System.Decimal? SKUVOLUME { get; set; }
        public System.Decimal? SKUVOLUMECL { get; set; }
        public System.Boolean SKUINDIVISIBLE { get; set; }
        public System.Boolean SKUDEFAULT { get; set; }
        
        //public MeasureWrapper Measure { get; set; }
        
        public List<ArtPriceWrapper> ArtPriceList { get; set; }
        public List<BarcodeWrapper> BarcodeList { get; set; }
        public List<SKU2TTEWrapper> TETYPE2SKU { get; set; }

        #endregion . Properties .
    }
}