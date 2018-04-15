using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wmsMLC.APS.wmsSI.Wrappers
{
    public class CargoOWBWrapper : BaseWrapper
    {
        public System.DateTime? CARGOOWBLOADEND { get; set; }
        public System.DateTime? CARGOOWBLOADBEGIN { get; set; }
        public System.Decimal? EXTERNALTRAFFICID_R { get; set; }
        public System.Decimal? CARGOOWBVOLUME { get; set; }
        public System.Decimal? CARGOOWBNET { get; set; }
        public System.Decimal? CARGOOWBID { get; set; }
        public System.Decimal? CARGOOWBCOUNT { get; set; }
        public System.Decimal? CARGOOWBBRUTTO { get; set; }
        public System.String VWORKERFIO { get; set; }
        public System.String VVEHICLERN { get; set; }
        public System.String CARGOOWBSTAMP { get; set; }
        public System.String CARGOOWBCONTAINER { get; set; }
        public AddressBookWrapper CARGOOWBUNLOADADDRESS { get; set; }
        public System.Decimal? INTERNALTRAFFICID_R { get; set; }
    }
}
