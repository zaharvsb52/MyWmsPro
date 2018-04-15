namespace wmsMLC.Business.Objects
{
    public class IsoCountry : WMSBusinessObject
    {
        #region .  Constants  .
        public const string COUNTRYALPHA2PropertyName = "COUNTRYALPHA2";
        public const string COUNTRYCODEPropertyName = "COUNTRYCODE";
        public const string COUNTRYNAMEENGPropertyName = "COUNTRYNAMEENG";
        public const string COUNTRYNAMERUSPropertyName = "COUNTRYNAMERUS";
        #endregion .  Constants  .

        #region .  Properties  .
        public string CountryCode
        {
            get { return GetProperty<string>(COUNTRYCODEPropertyName); }
            set { SetProperty(COUNTRYCODEPropertyName, value); }
        }
        public string CountryAlpha2
        {
            get { return GetProperty<string>(COUNTRYALPHA2PropertyName); }
            set { SetProperty(COUNTRYALPHA2PropertyName, value); }
        }

        public string COUNTRYNAMERUS
        {
            get { return GetProperty<string>(COUNTRYNAMERUSPropertyName); }
            set { SetProperty(COUNTRYNAMERUSPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}