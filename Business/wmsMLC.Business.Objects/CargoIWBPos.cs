namespace wmsMLC.Business.Objects
{
    public class CargoIWBPos : WMSBusinessObject
    {
        #region . Constants .
        public const string CARGOIWBID_RPropertyName = "CARGOIWBID_R";
        public const string CARGOIWBPOSCOUNTPropertyName = "CARGOIWBPOSCOUNT";
        public const string CARGOIWBPOSDESCPropertyName = "CARGOIWBPOSDESC";
        public const string CARGOIWBPOSIDPropertyName = "CARGOIWBPOSID";
        public const string CARGOIWBPOSTYPEPropertyName = "CARGOIWBPOSTYPE";
        public const string IWBID_RPropertyName = "IWBID_R";
        public const string QLFCODE_RPropertyName = "QLFCODE_R";
        public const string TETYPECODE_RPropertyName = "TETYPECODE_R";
        public const string CARGOIWBPOSBOXNUMBERPropertyName = "CARGOIWBPOSBOXNUMBER";
        #endregion . Constants .

        #region .  Properties  .
        public string CargoIwbPosType
        {
            get { return GetProperty<string>(CARGOIWBPOSTYPEPropertyName); }
            set { SetProperty(CARGOIWBPOSTYPEPropertyName, value); }
        }

        public decimal? CARGOIWBID_R
        {
            get { return GetProperty<decimal?>(CARGOIWBID_RPropertyName); }
            set { SetProperty(CARGOIWBID_RPropertyName, value); }
        }

        public decimal? IWBID_R
        {
            get { return GetProperty<decimal?>(IWBID_RPropertyName); }
            set { SetProperty(IWBID_RPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}