namespace wmsMLC.Business.Objects
{
    public class BillCalcConfig : WMSBusinessObject
    {
        #region .  Constants  .
        public const string SQLPropertyName = "CALCCONFIGSQL";
        #endregion .  Constants  .

        #region .  Properties  .
        public string CalcConfigSQL
        {
            get { return GetProperty<string>(SQLPropertyName); }
            set { SetProperty(SQLPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}