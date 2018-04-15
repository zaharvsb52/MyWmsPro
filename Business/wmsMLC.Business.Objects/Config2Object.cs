namespace wmsMLC.Business.Objects
{
    public class Config2Object : WMSBusinessObject
    {
        #region .  Constants  .

        public const string CONFIG2OBJECTOBJECTENTITYCODEPropertyName = "CONFIG2OBJECTOBJECTENTITYCODE";
        public const string CONFIG2OBJECTOBJECTNAMEPropertyName = "CONFIG2OBJECTOBJECTNAME";

        #endregion .  Constants  .

        #region .  Properties  .

        public string CONFIG2OBJECTOBJECTENTITYCODE
        {
            get { return GetProperty<string>(CONFIG2OBJECTOBJECTENTITYCODEPropertyName); }
            set { SetProperty(CONFIG2OBJECTOBJECTENTITYCODEPropertyName, value); }
        }

        public string CONFIG2OBJECTOBJECTNAME
        {
            get { return GetProperty<string>(CONFIG2OBJECTOBJECTNAMEPropertyName); }
            set { SetProperty(CONFIG2OBJECTOBJECTNAMEPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}