using System;

namespace wmsMLC.APS.wmsSI.Wrappers
{
    public class KitPosWrapper : BaseWrapper
    {
        #region . Properties .

        public Decimal? KITPOSID { get; set; }
        public String KITCODE_R { get; set; }
        public Decimal SKUID_R { get; set; }
        public Decimal KITPOSCOUNT { get; set; }
        public Decimal KITPOSPRIORITY { get; set; }
        //public ArtWrapper ART { get; set; }
        public String KITPOSARTNAME { get; set; }
        public String KITPOSMEASURE { get; set; }
        public Decimal KITPOSINSART { get; set; }

        #endregion
    }
}
