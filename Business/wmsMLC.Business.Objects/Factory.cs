namespace wmsMLC.Business.Objects
{
    public class Factory : WMSBusinessObject
    {
        #region .  Constants  .
        public const string FactoryIDPropertyName = "FACTORYID";
        public const string FactoryCodePropertyName = "FACTORYCODE";
        public const string FactoryNamePropertyName = "FACTORYNAME";
        public const string PARTNERID_RPropertyName = "PARTNERID_R";
        #endregion

        #region .  Properties  .
        public decimal? FactoryID
        {
            get { return GetProperty<decimal?>(FactoryIDPropertyName); }
            set { SetProperty(FactoryIDPropertyName, value); }
        }
        public string FactoryCode
        {
            get { return GetProperty<string>(FactoryCodePropertyName); }
            set { SetProperty(FactoryCodePropertyName, value); }
        }
        public string FactoryName
        {
            get { return GetProperty<string>(FactoryNamePropertyName); }
            set { SetProperty(FactoryNamePropertyName, value); }
        }
        public decimal? PARTNERID_R
        {
            get { return GetProperty<decimal?>(PARTNERID_RPropertyName); }
            set { SetProperty(PARTNERID_RPropertyName, value); }
        }

        #endregion

    }
}