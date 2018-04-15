using System;
using wmsMLC.General;

namespace wmsMLC.Business.Objects
{
    public class CargoIWB : WMSBusinessObject
    {
        #region .  Constants  .
        public const string CargoIWBIDPropertyName = "CARGOIWBID";
        public const string InternalTrafficIDPropertyName = "INTERNALTRAFFICID_R";
        public const string CargoIWBNetPropertyName = "CARGOIWBNET";
        public const string CargoIWBBruttoPropertyName = "CARGOIWBBRUTTO";
        public const string CargoIWBVolumePropertyName = "CARGOIWBVOLUME";
        public const string CargoIWBCountPropertyName = "CARGOIWBCOUNT";
        public const string CargoIWBLoadAddressPropertyName = "CARGOIWBLOADADDRESS";
        public const string CargoIWBLoadBeginPropertyName = "CARGOIWBLOADBEGIN";
        public const string CargoIWBLoadEndPropertyName = "CARGOIWBLOADEND";
        public const string CargoIWBStampPropertyName = "CARGOIWBSTAMP";
        public const string CargoIWBContainerPropertyName = "CARGOIWBCONTAINER";
        public const string CargoIWBPosLClientPropertyName = "CARGOIWBPOSLCLIENT";
        public const string CargoIWBPosLFactPropertyName = "CARGOIWBPOSLFACT";
        public const string VVEHICLERNPropertyName = "VVEHICLERN";
        public const string VWORKERFIOPropertyName = "VWORKERFIO";
        public const string MandantIDPropertyName = "MANDANTID";
        #endregion .  Constants  .

        #region .  Properties  .
        [IncludeInPartiallyGet]
        [XmlNotIgnoreAttribute]
        public AddressBook CargoIwbLoadAddress
        {
            get { return GetProperty<AddressBook>(CargoIWBLoadAddressPropertyName); }
            set { SetProperty(CargoIWBLoadAddressPropertyName, value); }
        }

        public WMSBusinessCollection<CargoIWBPos> CargoIWBPosLClient
        {
            get { return GetProperty<WMSBusinessCollection<CargoIWBPos>>(CargoIWBPosLClientPropertyName); }
            set { SetProperty(CargoIWBPosLClientPropertyName, value); }
        }

        public WMSBusinessCollection<CargoIWBPos> CargoIWBPosLFact
        {
            get { return GetProperty<WMSBusinessCollection<CargoIWBPos>>(CargoIWBPosLFactPropertyName); }
            set { SetProperty(CargoIWBPosLFactPropertyName, value); }
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
            get { return GetProperty<decimal?>(InternalTrafficIDPropertyName); }
            set { SetProperty(InternalTrafficIDPropertyName, value); }
        }

        public DateTime? LoadBegin
        {
            get { return GetProperty<DateTime?>(CargoIWBLoadBeginPropertyName); }
            set { SetProperty(CargoIWBLoadBeginPropertyName, value); }
        }

        public DateTime? LoadEnd
        {
            get { return GetProperty<DateTime?>(CargoIWBLoadEndPropertyName); }
            set { SetProperty(CargoIWBLoadEndPropertyName, value); }
        }

        public decimal? MandantID
        {
            get { return GetProperty<decimal?>(MandantIDPropertyName); }
            set { SetProperty(MandantIDPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}