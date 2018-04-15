using System.Collections.Generic;

namespace wmsMLC.APS.wmsSI.Wrappers
{
    public class BillContractWrapper : BaseWrapper
    {
        public System.DateTime? CONTRACTDATETILL { get; set; }
        public System.DateTime? CONTRACTDATEFROM { get; set; }
        public System.Decimal? CONTRACTOWNER { get; set; }
        public System.Decimal? CONTRACTID { get; set; }
        public System.Decimal? CONTRACTCUSTOMER { get; set; }
        public System.String VATTYPECODE_R { get; set; }
        public System.String CURRENCYCODE_R { get; set; }
        public System.String CONTRACTNUMBER { get; set; }
        public System.String CONTRACTHOSTREF { get; set; }
        public System.String CONTRACTDESC { get; set; }
        public List<BillOperation2ContractWrapper> BILLOPERATION2CONTRACTL { get; set; }

        public MandantWrapper CONTRACTOWNEROBJ { get; set; }
        public MandantWrapper CONTRACTCUSTOMEROBJ { get; set; }
    }
}
