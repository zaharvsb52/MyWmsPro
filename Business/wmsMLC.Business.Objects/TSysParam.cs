namespace wmsMLC.Business.Objects
{
    public class TSysParam : WMSBusinessObject
    {
        #region .  Constants  .
        public const string ParamCodePropertyName = "ParamCode";
        #endregion

        #region .  Properties  .
        public string ParamCode
        {
            get { return GetProperty<string>(ParamCodePropertyName); }
            set { SetProperty(ParamCodePropertyName, value); }
        }
        #endregion
    }
}