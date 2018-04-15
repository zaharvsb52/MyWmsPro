namespace wmsMLC.Business.Objects
{
    public class SupplyChain : WMSBusinessObject
    {
        #region .  Constants  .
        public const string STATUSCODE_RPropertyName = "STATUSCODE_R";
        #endregion .  Constants  .

        #region .  Properties  .
        public string StatusCode
        {
            get { return GetProperty<string>(STATUSCODE_RPropertyName); }
            set { SetProperty(STATUSCODE_RPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}