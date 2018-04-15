using System.Collections.Generic;

namespace wmsMLC.APS.wmsSI.Wrappers
{
    public class BillOperation2ContractWrapper : BaseWrapper
    {
        public System.String BILLOPERATION2CONTRACTANALYTICSCODE { get; set; }
        public System.String BILLOPERATION2CONTRACTBILLERCODE { get; set; }
        public System.Decimal? BILLOPERATION2CONTRACTCONTRACTID { get; set; }
        public System.String BILLOPERATION2CONTRACTOPERATIONCODE { get; set; }
        public List<BillOperationCauseWrapper> BILLOPERATIONCAUSEL { get; set; }
        public List<BillScale2O2CWrapper> BILLSCALE2O2CL { get; set; }
        public List<BillTariffWrapper> BILLTARIFFL { get; set; }
        public System.String OPERATION2CONTRACTDESC { get; set; }
        public System.Decimal? OPERATION2CONTRACTID { get; set; }
        public System.String OPERATION2CONTRACTNAME { get; set; }
        public BillAnalyticsWrapper BILLANALYTICS { get; set; }

    }
}
