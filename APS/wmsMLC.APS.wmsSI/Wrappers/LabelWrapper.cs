using System.Collections.Generic;

namespace wmsMLC.APS.wmsSI.Wrappers
{
    public class LabelWrapper : BaseWrapper
    {
        #region . Properties .

        public System.String LABELCODE { get; set; }
        public System.String LABELNAME { get; set; }
        public System.String ARTCODE { get; set; }
        public System.String ARTNAME { get; set; }
        public System.String REPORT_R { get; set; }
        public System.Decimal? PARTNERID_R { get; set; }
        public List<LabelParamsWrapper> LabelParamsL { get; set; }
        public string MandantCode { get; set; }
        public System.Decimal? MANDANTID { get; set; }

        #endregion
    }
}
