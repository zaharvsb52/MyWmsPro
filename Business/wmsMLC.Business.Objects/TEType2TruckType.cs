namespace wmsMLC.Business.Objects
{
    public class TEType2TruckType : WMSBusinessObject
    {
        #region .  Constants  .
        public const string TeType2TruckTypeTeTypeCodePropertyName = "TETYPE2TRUCKTYPETETYPECODE";
        public const string TeType2TruckTypeCountropertyName = "TETYPE2TRUCKTYPECOUNT";
        #endregion .  Constants  .

        #region .  Properties  .
        public string TeType2TruckTypeTeTypeCode
        {
            get { return GetProperty<string>(TeType2TruckTypeTeTypeCodePropertyName); }
            set { SetProperty(TeType2TruckTypeTeTypeCodePropertyName, value); }
        }

        public decimal TeType2TruckTypeCount
        {
            get { return GetProperty<decimal>(TeType2TruckTypeCountropertyName); }
            set { SetProperty(TeType2TruckTypeCountropertyName, value); }
        }
        #endregion .  Properties  .
    }
}