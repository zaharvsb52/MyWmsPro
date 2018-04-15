namespace wmsMLC.Business.Objects
{
    public class User2Mandant : WMSBusinessObject
    {
        #region .  Constants  .
        public const string User2MandantIsActivePropertyName = "USER2MANDANTISACTIVE";
        public const string MandantIDPropertyName = "MANDANTID";
        public const string MandantCodePropertyName = "VMANDANTCODE";
        public const string MandantNamePropertyName = "VMANDANTNAME";
        #endregion

        #region .  Properties  .
        public bool User2MandantIsActive
        {
            get { return GetProperty<bool>(User2MandantIsActivePropertyName); }
            set { SetProperty(User2MandantIsActivePropertyName, value); }
        }

        public decimal MandantID
        {
            get { return GetProperty<decimal>(MandantIDPropertyName); }
            set { SetProperty(MandantIDPropertyName, value); }
        }

        public string MandantCode
        {
            get { return GetProperty<string>(MandantCodePropertyName); }
            set { SetProperty(MandantCodePropertyName, value); }
        }

        public string MandantName
        {
            get { return GetProperty<string>(MandantNamePropertyName); }
            set { SetProperty(MandantNamePropertyName, value); }
        }
        #endregion

    }
}