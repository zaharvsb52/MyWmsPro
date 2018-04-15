namespace wmsMLC.Business.Objects
{
    public class LabelParams : WMSBusinessObject
    {
        #region .  Constants  .
        public const string LabelCodeRPropertyName = "LABELCODE_R";
        public const string LabelParamsNamePropertyName = "LABELPARAMSNAME";
        public const string LabelParamIDPropertyName = "LABELPARAMSID";
        #endregion

        #region .  Properties  .
        public string LabelCode_R
        {
            get { return GetProperty<string>(LabelCodeRPropertyName); }
            set { SetProperty(LabelCodeRPropertyName, value); }
        }
        public string LabelParamsName
        {
            get { return GetProperty<string>(LabelParamsNamePropertyName); }
            set { SetProperty(LabelParamsNamePropertyName, value); }
        }
        public decimal? LabelParamsID
        {
            get { return GetProperty<decimal>(LabelParamIDPropertyName); }
            set { SetProperty(LabelParamIDPropertyName, value); }
        }
        #endregion
    }
}
