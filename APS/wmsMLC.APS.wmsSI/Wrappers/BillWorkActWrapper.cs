using System.Collections.Generic;

namespace wmsMLC.APS.wmsSI.Wrappers
{
    public class BillWorkActWrapper : BaseWrapper
    {
        #region . Properties .

        public System.Decimal? CONTRACTID_R { get; set; }
        public System.DateTime? WORKACTDATE { get; set; }
        public System.DateTime? WORKACTDATEFROM { get; set; }
        public System.DateTime? WORKACTDATETILL { get; set; }
        public System.DateTime? WORKACTFIXDATE { get; set; }
        public System.String WORKACTHOSTREF { get; set; }
        public System.Decimal? WORKACTID { get; set; }
        public System.DateTime? WORKACTPOSTINGDATE { get; set; }
        public System.Double? WORKACTTOTALAMOUNT { get; set; }
        public List<BillWorkActDetailExWrapper> WORKACTDETAILEXL { get; set; }

        public BillContractWrapper CONTRACT { get; set; }

        public System.String WORKACTPOSTINGNUMBER { get; set; }
        public System.String STATUSCODE_R { get; set; }
        public ErrorWrapper ERROR { get; set; }

        #endregion
    }
}