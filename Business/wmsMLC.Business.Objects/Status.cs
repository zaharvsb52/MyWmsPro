namespace wmsMLC.Business.Objects
{
    public class Status : WMSBusinessObject
    {
        #region .  Constants  .
        public const string StatusCodePropertyName = "STATUSCODE";
        public const string StatusNamePropertyName = "STATUSNAME";
        #endregion .  Constants  .

        #region .  Properties  .
        public string StatusCode
        {
            get { return GetProperty<string>(StatusCodePropertyName); }
            set { SetProperty(StatusCodePropertyName, value); }
        }
        public string StatusName
        {
            get { return GetProperty<string>(StatusNamePropertyName); }
            set { SetProperty(StatusNamePropertyName, value); }
        }
        #endregion
    }
}