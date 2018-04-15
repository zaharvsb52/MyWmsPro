namespace wmsMLC.Business.Objects
{
    public class PMMethod2Operation : WMSBusinessObject
    {
        #region .  Constants  .

        public const string PMMETHOD2OPERATIONCONFIG2OBJECTIDPropertyName = "PMMETHOD2OPERATIONCONFIG2OBJECTID";
        public const string PMMETHOD2OPERATIONPMMETHODCODEPropertyName = "PMMETHOD2OPERATIONPMMETHODCODE";
        public const string PMMETHOD2OPERATIONOPERATIONCODEPropertyName = "PMMETHOD2OPERATIONOPERATIONCODE";
        public const string VOBJECTENTITYCODEPropertyName = "VOBJECTENTITYCODE";
        public const string VOBJECTNAMEPropertyName = "VOBJECTNAME";
        public const string PMMETHOD2OPERATIONBYPRODUCTPropertyName = "PMMETHOD2OPERATIONBYPRODUCT";
        public const string PMMETHOD2OPERATIONINPUTMASKPropertyName = "PMMETHOD2OPERATIONINPUTMASK";
        public const string PMMETHOD2OPERATIONINPUTMASSPropertyName = "PMMETHOD2OPERATIONINPUTMASS";

        #endregion .  Constants  .

        #region .  Properties  .

        public decimal? PMMETHOD2OPERATIONCONFIG2OBJECTID
        {
            get { return GetProperty<decimal?>(PMMETHOD2OPERATIONCONFIG2OBJECTIDPropertyName); }
            set { SetProperty(PMMETHOD2OPERATIONCONFIG2OBJECTIDPropertyName, value); }
        }

        public string PMMETHOD2OPERATIONPMMETHODCODE
        {
            get { return GetProperty<string>(PMMETHOD2OPERATIONPMMETHODCODEPropertyName); }
            set { SetProperty(PMMETHOD2OPERATIONPMMETHODCODEPropertyName, value); }
        }

        public string PMMETHOD2OPERATIONOPERATIONCODE
        {
            get { return GetProperty<string>(PMMETHOD2OPERATIONOPERATIONCODEPropertyName); }
            set { SetProperty(PMMETHOD2OPERATIONOPERATIONCODEPropertyName, value); }
        }

        public string VOBJECTENTITYCODE
        {
            get { return GetProperty<string>(VOBJECTENTITYCODEPropertyName); }
            set { SetProperty(VOBJECTENTITYCODEPropertyName, value); }
        }

        public string VOBJECTNAME
        {
            get { return GetProperty<string>(VOBJECTNAMEPropertyName); }
            set { SetProperty(VOBJECTNAMEPropertyName, value); }
        }

        public bool? PMMETHOD2OPERATIONBYPRODUCT
        {
            get { return GetProperty<bool?>(PMMETHOD2OPERATIONBYPRODUCTPropertyName); }
            set { SetProperty(PMMETHOD2OPERATIONBYPRODUCTPropertyName, value); }
        }

        public bool? PMMETHOD2OPERATIONINPUTMASK
        {
            get { return GetProperty<bool?>(PMMETHOD2OPERATIONINPUTMASKPropertyName); }
            set { SetProperty(PMMETHOD2OPERATIONINPUTMASKPropertyName, value); }
        }

        public bool? PMMETHOD2OPERATIONINPUTMASS
        {
            get { return GetProperty<bool?>(PMMETHOD2OPERATIONINPUTMASSPropertyName); }
            set { SetProperty(PMMETHOD2OPERATIONINPUTMASSPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}