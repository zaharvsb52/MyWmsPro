using System.Collections.Generic;

namespace wmsMLC.APS.wmsSI.Wrappers
{
    public class KitWrapper : BaseWrapper
    {
        #region . Properties .
        public string KITCODE { get; set; }
        public string KITTYPECODE_R { get; set; }
        public string ARTCODE_R { get; set; }
        public string ARTNAME { get; set; }
        public string ARTDESC { get; set; }
        public string ARTABCD { get; set; }
        public string KITMEASURE { get; set; }
        public decimal KITPRIORITYIN { get; set; }
        public decimal KITPRIORITYOUT { get; set; }
        public KitTypeWrapper KITTYPE { get; set; }
        public List<KitPosWrapper> KITPOS { get; set; }
        public decimal? MANDANTID { get; set; }
        public decimal KITCOUNT { get; set; }
        public decimal KITUPDATE { get; set; }
        public decimal KITINSART { get; set; }

        public string MandantCode { get; set; }

        #endregion . Properties .
    }
}
