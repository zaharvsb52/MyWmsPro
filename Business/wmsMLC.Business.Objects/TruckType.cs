namespace wmsMLC.Business.Objects
{
    public class TruckType : WMSBusinessObject
    {
        #region .  Constants  .
        public const string TruckTypeWeightMaxPropertyName = "TRUCKTYPEWEIGHTMAX";
        public const string TeType2TruckTypeLPropertyName = "TETYPE2TRUCKTYPEL";
        #endregion .  Constants  .

        #region .  Properties  .
        public decimal TruckTypeWeightMax
        {
            get { return GetProperty<decimal>(TruckTypeWeightMaxPropertyName); }
            set { SetProperty(TruckTypeWeightMaxPropertyName, value); }
        }

        public WMSBusinessCollection<TEType2TruckType> TeType2TruckTypeL
        {
            get { return GetProperty<WMSBusinessCollection<TEType2TruckType>>(TeType2TruckTypeLPropertyName); }
            set { SetProperty(TeType2TruckTypeLPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}