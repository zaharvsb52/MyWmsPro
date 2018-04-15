namespace wmsMLC.Business.Objects
{
    public class IWB2Cargo : WMSBusinessObject
    {
        #region .  Constants  .
        public const string IWB2CARGOIWBIDPropertyName = "IWB2CARGOIWBID";
        public const string IWB2CARGOCARGOIWBIDPropertyName = "IWB2CARGOCARGOIWBID";
        #endregion .  Constants  .

        #region .  Properties  .
        public decimal? IWBID
        {
            get { return GetProperty<decimal?>(IWB2CARGOIWBIDPropertyName); }
            set { SetProperty(IWB2CARGOIWBIDPropertyName, value); }
        }
        public decimal? CARGOIWBID
        {
            get { return GetProperty<decimal?>(IWB2CARGOCARGOIWBIDPropertyName); }
            set { SetProperty(IWB2CARGOCARGOIWBIDPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}