namespace wmsMLC.Business.Objects
{
    public class ObjectLookUp : WMSBusinessObject
    {
        #region .  Constants  .
        public const string ObjectLookupCodePropertyName = "OBJECTLOOKUPCODE";
        public const string ObjectLookupSourcePropertyName = "OBJECTLOOKUPSOURCE";
        public const string ObjectLookupDisplayPropertyName = "OBJECTLOOKUPDISPLAY";
        public const string ObjectLookupPkeyPropertyName = "OBJECTLOOKUPPKEY";
        public const string ObjectLookupFilterPropertyName = "OBJECTLOOKUPFILTER";
        public const string ObjectLookupSimplePropertyName = "OBJECTLOOKUPSIMPLE";
        public const string ObjectLookupFetchRowCountPropertyName = "OBJECTLOOKUPFETCHROWCOUNT";
        #endregion .  Constants  .

        #region .  Properties  .
        public string ObjectLookupCode
        {
            get { return GetProperty<string>(ObjectLookupCodePropertyName); }
            set { SetProperty(ObjectLookupCodePropertyName, value); }
        }

        public string ObjectLookupSource
        {
            get { return GetProperty<string>(ObjectLookupSourcePropertyName); }
            set { SetProperty(ObjectLookupSourcePropertyName, value); }
        }

        public string ObjectLookupDisplay
        {
            get { return GetProperty<string>(ObjectLookupDisplayPropertyName); }
            set { SetProperty(ObjectLookupDisplayPropertyName, value); }
        }

        public string ObjectLookupPkey
        {
            get { return GetProperty<string>(ObjectLookupPkeyPropertyName); }
            set { SetProperty(ObjectLookupPkeyPropertyName, value); }
        }

        public string ObjectLookupFilter
        {
            get { return GetProperty<string>(ObjectLookupFilterPropertyName); }
            set { SetProperty(ObjectLookupFilterPropertyName, value); }
        }

        public decimal? ObjectLookupFetchRowCount
        {
            get { return GetProperty<decimal?>(ObjectLookupFetchRowCountPropertyName); }
            set { SetProperty(ObjectLookupFetchRowCountPropertyName, value); }
        }

        public decimal ObjectLookupSimple
        {
            get { return GetProperty<decimal>(ObjectLookupSimplePropertyName); }
            set { SetProperty(ObjectLookupSimplePropertyName, value); }
        }
        #endregion  .  Properties  .
    }
}