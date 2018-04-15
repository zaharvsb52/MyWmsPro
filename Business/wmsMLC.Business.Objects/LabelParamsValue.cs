namespace wmsMLC.Business.Objects
{
    public class LabelParamsValue : WMSBusinessObject
    {
        #region .  Constants  .
        public const string LabelUseIDRPropertyName = "LABELUSEID_R";
        public const string LabelParamsTextPropertyName = "LABELPARAMSVALUETEXT";
        public const string LabelParamsIDRPropertyName = "LABELPARAMSID_R";
        #endregion

        #region .  Properties  .
        public decimal? LabelUseIDR
        {
            get { return GetProperty<decimal>(LabelUseIDRPropertyName); }
            set { SetProperty(LabelUseIDRPropertyName, value); }
        }
        public string LabelParamsText
        {
            get { return GetProperty<string>(LabelParamsTextPropertyName); }
            set { SetProperty(LabelParamsTextPropertyName, value); }
        }
        public decimal? LabelParamsIDR
        {
            get { return GetProperty<decimal>(LabelParamsIDRPropertyName); }
            set { SetProperty(LabelParamsIDRPropertyName, value); }
        }
        #endregion
    }
}
