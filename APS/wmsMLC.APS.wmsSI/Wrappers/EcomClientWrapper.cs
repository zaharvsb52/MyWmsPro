using System.Collections.Generic;

namespace wmsMLC.APS.wmsSI.Wrappers
{
    public class EcomClientWrapper : BaseWrapper
    {
        public System.String ClientLastName { get; set; }
        public System.String ClientName { get; set; }
        public System.String ClientMiddleName { get; set; }
        public System.String ClientPhoneMobile { get; set; }
        public System.String ClientPhoneWork { get; set; }
        public System.String ClientPhoneInternal { get; set; }
        public System.String ClientPhoneHome { get; set; }
        public System.String ClientEmail { get; set; }
        public System.String ClientHostRef { get; set; }
        public List<AddressBookWrapper> AddressL { get; set; }
    }
}