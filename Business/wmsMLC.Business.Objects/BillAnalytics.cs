namespace wmsMLC.Business.Objects
{
    public class BillAnalytics : WMSBusinessObject
    {
        public const string AnalitycsCodePropertyName = "ANALYTICSCODE";
        public const string AnalitycsNamePropertyName = "ANALYTICSNAME";
        public const string AnalitycsDescPropertyName = "ANALYTICSDESC";
        public const string AnalitycsCodeRPropertyName = "ANALYTICSCODE_R";

        #region .  Properties  .
        public string AnalitycsCode
        {
            get { return GetProperty<string>(AnalitycsDescPropertyName); }
            set { SetProperty(AnalitycsDescPropertyName, value); }
        }
        public string AnalitycsCodeR
        {
            get { return GetProperty<string>(AnalitycsCodeRPropertyName); }
            set { SetProperty(AnalitycsCodeRPropertyName, value); }
        }
        #endregion
    }
}