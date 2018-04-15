namespace wmsMLC.Business.Objects
{
    public class Transit : WMSBusinessObject
    {
        #region .  Constants  .
        public const string TransitIDPropertyName = "TRANSITID";
        public const string TransitNamePropertyName = "TRANSITNAME";
        public const string TransitDescPropertyName = "TRANSITDESC";
        public const string TransitEntityPropertyName = "TRANSIT2ENTITY";
        public const string MANDANTIDPropertyName = "MANDANTID";
        #endregion .  Constants  .

        #region .  Properties  .
        public decimal? MandantID
        {
            get { return GetProperty<decimal>(MANDANTIDPropertyName); }
            set { SetProperty(MANDANTIDPropertyName, value); }
        }
        public decimal TransitID
        {
            get { return GetProperty<decimal>(TransitIDPropertyName); }
            set { SetProperty(TransitIDPropertyName, value); }
        }
        public string TransitName
        {
            get { return GetProperty<string>(TransitNamePropertyName); }
            set { SetProperty(TransitNamePropertyName, value); }
        }
        public string TransitDesc
        {
            get { return GetProperty<string>(TransitDescPropertyName); }
            set { SetProperty(TransitDescPropertyName, value); }
        }
        public string TransitEntity
        {
            get { return GetProperty<string>(TransitEntityPropertyName); }
            set { SetProperty(TransitEntityPropertyName, value); }
        }
        #endregion
    }
}