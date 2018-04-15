using System.Collections.Generic;

namespace wmsMLC.APS.wmsSI.Wrappers
{
    public class CargoIWBWrapper : BaseWrapper
    {
        #region . Properties .

        public System.DateTime? CARGOIWBLOADEND { get; set; }
        public System.DateTime? CARGOIWBLOADBEGIN { get; set; }
        public System.Decimal? MANDANTID { get; set; }
        public System.Decimal? EXTERNALTRAFFICID_R { get; set; }
        public System.Decimal? CARGOIWBVOLUME { get; set; }
        public System.Decimal? CARGOIWBNET { get; set; }
        public System.Decimal? CARGOIWBID { get; set; }
        public System.Decimal? CARGOIWBCOUNT { get; set; }
        public System.Decimal? CARGOIWBBRUTTO { get; set; }
        public System.String VWORKERFIO { get; set; }
        public System.String VVEHICLERN { get; set; }
        public System.String CARGOIWBSTAMP { get; set; }
        public System.String CARGOIWBCONTAINER { get; set; }
        public AddressBookWrapper CARGOIWBLOADADDRESS { get; set; }
        public List<CargoIWBPosWrapper> CARGOIWBPOSLFACT { get; set; }
        public List<CargoIWBPosWrapper> CARGOIWBPOSLCLIENT { get; set; }
        public System.Decimal? INTERNALTRAFFICID_R { get; set; }

        #endregion
    }
}
