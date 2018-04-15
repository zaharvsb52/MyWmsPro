namespace wmsMLC.Business.Objects
{
    public class PartnerColor : WMSBusinessObject
    {
        #region .  Constants  .

        public const string MANDANTIDPropertyName = "MANDANTID";
        public const string PARTNERCOLORCODEPropertyName = "PARTNERCOLORCODE";
        public const string PARTNERCOLORNAMEPropertyName = "PARTNERCOLORNAME";

        #endregion .  Constants  .

        #region .  Properties  .
        public decimal? MANDANTID
        {
            get { return GetProperty<decimal?>(MANDANTIDPropertyName); }
            set { SetProperty(MANDANTIDPropertyName, value); }
        }

        public string PARTNERCOLORCODE
        {
            get { return GetProperty<string>(PARTNERCOLORCODEPropertyName); }
            set { SetProperty(PARTNERCOLORCODEPropertyName, value); }
        }

        public string PARTNERCOLORNAME
        {
            get { return GetProperty<string>(PARTNERCOLORNAMEPropertyName); }
            set { SetProperty(PARTNERCOLORNAMEPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}