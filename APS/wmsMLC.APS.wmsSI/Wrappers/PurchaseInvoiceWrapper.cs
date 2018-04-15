using System;
using System.Collections.Generic;

namespace wmsMLC.APS.wmsSI.Wrappers
{
    public class PurchaseInvoiceWrapper : BaseWrapper
    {
        public decimal? OWBID_R { get; set; }
        public decimal? MANDANTID { get; set; }
        public decimal? IWBSENDER { get; set; }
        public decimal? IWBRECIPIENT { get; set; }
        public decimal IWBPRIORITY { get; set; }
        public decimal? IWBPAYER { get; set; }
        public decimal? IWBID { get; set; }
        public string STATUSCODE_R_NAME { get; set; }
        public string STATUSCODE_R { get; set; }
        public string IWBSENDER_NAME { get; set; }
        public string IWBRECIPIENT_NAME { get; set; }
        public string IWBPAYER_NAME { get; set; }
        public string IWBNAME { get; set; }
        public string IWBHOSTREF { get; set; }
        public DateTime? IWBINDATEPLAN { get; set; }
        public string IWBINVOICENUMBER { get; set; }
        public List<IWBCpvWrapper> CUSTOMPARAMVAL { get; set; }
        public List<IWBPosWrapper> IWBPOSL { get; set; }
        public string IWBFACTORY { get; set; }
        public decimal? IWBCOUNTPOS { get; set; }
        public string IWBTYPE { get; set; }
        public decimal? IWBREPLACEKITS { get; set; }
        public decimal? IWBALLOWBASE { get; set; }
        public string IWBSENDER_CODE { get; set; }
        public string IWBRECIPIENT_CODE { get; set; }
        public decimal? IWBCREATE_SENDER { get; set; }
        public string IWBCMRNUMBER { get; set; }
        public decimal? FACTORYID_R { get; set; }
        public decimal? IWBCHECKATTR { get; set; }
        public decimal? IWBCONVERTSKU { get; set; }
        public DateTime? IWBCMRDATE { get; set; }
        public string MandantCode { get; set; }
        public List<TransitDataWrapper> TRANSITDATAL { get; set; }
        public decimal? IWBRECREATE { get; set; }
        public decimal? IWBCHECKMULTIPLE { get; set; }
        public string IWBDESC { get; set; }
        public string IWBDELGROUP { get; set; }
        public decimal? IWBUPDATE_SENDER { get; set; }
        public string IWBARRIVED { get; set; }
        public string IWBDEPARTED { get; set; }
        public string IWBLOADBEGIN { get; set; }
        public string IWBLOADEND { get; set; }
    }
}
