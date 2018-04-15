namespace wmsMLC.Business.Objects
{
    public class PlaceType : WMSBusinessObject
    {
        #region .  Constants  .
        public const string MaxWeightPropertyName = "PLACETYPEMAXWEIGHT";
        public const string HeightPropertyName = "PLACETYPEHEIGHT";
        public const string WidthPropertyName = "PLACETYPEWIDTH";
        public const string LengthPropertyName = "PLACETYPELENGTH";
        public const string CapacityPropertyName = "PLACETYPECAPACITY";

        #endregion .  Constants  .

        #region .  Properties  .
        public decimal MaxWeight
        {
            get { return GetProperty<decimal>(MaxWeightPropertyName); }
            set { SetProperty(MaxWeightPropertyName, value); }
        }
        public decimal Height
        {
            get { return GetProperty<decimal>(HeightPropertyName); }
            set { SetProperty(HeightPropertyName, value); }
        }
        public decimal Width
        {
            get { return GetProperty<decimal>(WidthPropertyName); }
            set { SetProperty(WidthPropertyName, value); }
        }
        public decimal Length
        {
            get { return GetProperty<decimal>(LengthPropertyName); }
            set { SetProperty(LengthPropertyName, value); }
        }
        public decimal Capacity
        {
            get { return GetProperty<decimal>(CapacityPropertyName); }
            set { SetProperty(CapacityPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}