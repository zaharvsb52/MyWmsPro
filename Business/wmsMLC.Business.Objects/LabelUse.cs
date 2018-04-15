namespace wmsMLC.Business.Objects
{
    public class LabelUse : WMSBusinessObject
    {
        #region .  Constants  .
        public const string LabelUseIDPropertyName = "LABELUSEID";
        public const string LabelCode_RPropertyName = "LABELCODE_R";
        public const string ArtCode_RPropertyName = "ARTCODE_R";
        public const string SKUID_RPropertyName = "SKUID_R";
        #endregion

        #region .  Properties  .
        public decimal? LabelUseID
        {
            get { return GetProperty<decimal>(LabelUseIDPropertyName); }
            set { SetProperty(LabelUseIDPropertyName, value); }
        }
        public string LabelCode_R
        {
            get { return GetProperty<string>(LabelCode_RPropertyName); }
            set { SetProperty(LabelCode_RPropertyName, value); }
        }
        public string ArtCode_R
        {
            get { return GetProperty<string>(ArtCode_RPropertyName); }
            set { SetProperty(ArtCode_RPropertyName, value); }
        }
        public decimal? SKUID_R
        {
            get { return GetProperty<decimal>(SKUID_RPropertyName); }
            set { SetProperty(SKUID_RPropertyName, value); }
        }
        #endregion
    }
}
