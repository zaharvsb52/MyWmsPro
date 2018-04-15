namespace wmsMLC.Business.Objects
{
    public class SysService : WMSBusinessObject
    {
        #region .  Constants  .
        public const string ServiceCodePropertyName = "SERVICECODE";
        #endregion

        #region .  Properties  .
        public string ServiceCode
        {
            get { return GetProperty<string>(ServiceCodePropertyName); }
            set { SetProperty(ServiceCodePropertyName, value); }
        }
        #endregion
    }
}