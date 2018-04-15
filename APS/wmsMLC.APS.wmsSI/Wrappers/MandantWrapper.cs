using System.Collections.Generic;

namespace wmsMLC.APS.wmsSI.Wrappers
{
    public class MandantWrapper : BaseWrapper
    {
        public List<MandantGpvWrapper> GLOBALPARAMVAL { get; set; }
        public System.Boolean MANDANTLOCKED { get; set; }
        public System.DateTime? MANDANTDATECONTRACT { get; set; }
        public System.Decimal? MANDANTID { get; set; }
        public System.String MANDANTSETTLEMENTACCOUNT { get; set; }
        public System.String MANDANTPHONE { get; set; }
        public System.String MANDANTOKVED { get; set; }
        public System.String MANDANTOKPO { get; set; }
        public System.String MANDANTOGRN { get; set; }
        public System.String MANDANTNAME { get; set; }
        public System.String MANDANTKPP { get; set; }
        public System.String MANDANTINN { get; set; }
        public System.String MANDANTHOSTREF { get; set; }
        public System.String MANDANTFULLNAME { get; set; }
        public System.String MANDANTFAX { get; set; }
        public System.String MANDANTEMAIL { get; set; }
        public System.String MANDANTCORRESPONDENTACCOUNT { get; set; }
        public System.String MANDANTCONTRACT { get; set; }
        public System.String MANDANTCODE { get; set; }
        public System.String MANDANTBIK { get; set; }
        public List<AddressBookWrapper> ADDRESS { get; set; }
    }
}
