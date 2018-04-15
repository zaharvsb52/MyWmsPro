namespace wmsMLC.Business.Objects
{
    public class Label : WMSBusinessObject
    {
        #region .  Constants  .
        public const string LabelCodePropertyName = "LABELCODE";
        public const string LabelNamePropertyName = "LABELNAME";
        public const string MANDANTIDPropertyName = "MANDANTID";
        #endregion

        #region .  Properties  .
        public string LabelCode
        {
            get { return GetProperty<string>(LabelCodePropertyName); }
            set { SetProperty(LabelCodePropertyName, value); }
        }
        public string LabelName
        {
            get { return GetProperty<string>(LabelNamePropertyName); }
            set { SetProperty(LabelNamePropertyName, value); }
        }
        public decimal? MandantID
        {
            get { return GetProperty<decimal>(MANDANTIDPropertyName); }
            set { SetProperty(MANDANTIDPropertyName, value); }
        }
        #endregion
    }
}
