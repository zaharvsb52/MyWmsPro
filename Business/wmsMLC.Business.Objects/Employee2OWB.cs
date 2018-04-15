namespace wmsMLC.Business.Objects
{
    public class Employee2OWB : WMSBusinessObject
    {
        #region .  Constants  .
        //TODO: разобраться, почему имена этих параметров не совпадают с SO
        public const string EMPLOYEE2OWBEMPLOYEEIDPropertyName = "EMPLOYEE2OWBEMPLOYEEID";
        public const string EMPLOYEE2OWBOWBIDPropertyName = "EMPLOYEE2OWBOWBID";

        public const string EMPLOYEE2OWBIDPropertyName = "EMPLOYEE2OWBID";
        public const string VEMPLOYEEPropertyName = "VEMPLOYEE";
        public const string VOWBNAMEPropertyName = "VOWBNAME";
        #endregion

        #region .  Properties  .
        public System.Decimal? EMPLOYEE2OWBID
        {
            get { return GetProperty<System.Decimal?>(EMPLOYEE2OWBIDPropertyName); }
            set { SetProperty(EMPLOYEE2OWBIDPropertyName, value); }
        }
        public System.Decimal? EMPLOYEE2OWBEMPLOYEEID
        {
            get { return GetProperty<System.Decimal?>(EMPLOYEE2OWBEMPLOYEEIDPropertyName); }
            set { SetProperty(EMPLOYEE2OWBEMPLOYEEIDPropertyName, value); }
        }
        public System.Decimal? EMPLOYEE2OWBOWBID
        {
            get { return GetProperty<System.Decimal?>(EMPLOYEE2OWBOWBIDPropertyName); }
            set { SetProperty(EMPLOYEE2OWBOWBIDPropertyName, value); }
        }
        public System.String VEMPLOYEE
        {
            get { return GetProperty<System.String>(VEMPLOYEEPropertyName); }
            set { SetProperty(VEMPLOYEEPropertyName, value); }
        }
        public System.String VOWBNAME
        {
            get { return GetProperty<System.String>(VOWBNAMEPropertyName); }
            set { SetProperty(VOWBNAMEPropertyName, value); }
        }
        #endregion
    }
}