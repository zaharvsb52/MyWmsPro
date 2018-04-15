using wmsMLC.General;

namespace wmsMLC.Business.Objects
{
    public class CargoOWB : WMSBusinessObject
    {
        #region .  Constants  .
        public const string CARGOOWBBRUTTOPropertyName = "CARGOOWBBRUTTO";
        public const string CARGOOWBCONTAINERPropertyName = "CARGOOWBCONTAINER";
        public const string CARGOOWBCOUNTPropertyName = "CARGOOWBCOUNT";
        public const string CARGOOWBIDPropertyName = "CARGOOWBID";
        public const string CARGOOWBLOADBEGINPropertyName = "CARGOOWBLOADBEGIN";
        public const string CARGOOWBLOADENDPropertyName = "CARGOOWBLOADEND";
        public const string CARGOOWBNETPropertyName = "CARGOOWBNET";
        public const string CARGOOWBSTAMPPropertyName = "CARGOOWBSTAMP";
        public const string CARGOOWBUNLOADADDRESSPropertyName = "CARGOOWBUNLOADADDRESS";
        public const string CARGOOWBVOLUMEPropertyName = "CARGOOWBVOLUME";
        public const string INTERNALTRAFFICID_RPropertyName = "INTERNALTRAFFICID_R";
        public const string OWB2CARGOLPropertyName = "OWB2CARGOL";
        public const string VVEHICLERNPropertyName = "VVEHICLERN";
        public const string VWORKERFIOPropertyName = "VWORKERFIO";

        #endregion .  Constants  .

        #region .  Properties  .
        [IncludeInPartiallyGet]
        [XmlNotIgnoreAttribute]
        public AddressBook CargoOwbUnloadAddress
        {
            get { return GetProperty<AddressBook>(CARGOOWBUNLOADADDRESSPropertyName); }
            set { SetProperty(CARGOOWBUNLOADADDRESSPropertyName, value); }
        }

        public string Vehicle
        {
            get { return GetProperty<string>(VVEHICLERNPropertyName); }
            set { SetProperty(VVEHICLERNPropertyName, value); }
        }

        public string WorkerFIO
        {
            get { return GetProperty<string>(VWORKERFIOPropertyName); }
            set { SetProperty(VWORKERFIOPropertyName, value); }
        }

        public decimal? InternalTrafficID
        {
            get { return GetProperty<decimal?>(INTERNALTRAFFICID_RPropertyName); }
            set { SetProperty(INTERNALTRAFFICID_RPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}