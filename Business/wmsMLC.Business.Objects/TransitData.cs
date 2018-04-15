namespace wmsMLC.Business.Objects
{
    public class TransitData : WMSBusinessObject
    {
        #region .  Constants  .
        public const string TransitDataKeyPropertyName = "TRANSITDATAKEY";
        public const string TransitDataValuePropertyName = "TRANSITDATAVALUE";
        public const string TransitIDPropertyName = "TRANSITID_R";
        #endregion .  Constants  .

        #region .  Properties  .
        public decimal? TransitID
        {
            get { return GetProperty<decimal>(TransitIDPropertyName); }
            set { SetProperty(TransitIDPropertyName, value); }
        }
        public string TransitDataKey
        {
            get { return GetProperty<string>(TransitDataKeyPropertyName); }
            set { SetProperty(TransitDataKeyPropertyName, value); }
        }
        public string TransitDataValue
        {
            get { return GetProperty<string>(TransitDataValuePropertyName); }
            set { SetProperty(TransitDataValuePropertyName, value); }
        }
        #endregion
    }
}