namespace wmsMLC.Business.Objects
{
    public class Partner2Group : WMSBusinessObject
    {
        #region .  Constants  .

        public const string PARTNERGROUPIDPropertyName = "PARTNER2GROUPPARTNERGROUPID";
        public const string PARTNERIDPropertyName = "PARTNER2GROUPPARTNERID";

        #endregion .  Constants  .

        #region .  Properties  .
        public decimal? PartnerGroupId
        {
            get { return GetProperty<decimal?>(PARTNERGROUPIDPropertyName); }
            set { SetProperty(PARTNERGROUPIDPropertyName, value); }
        }
        public decimal? PartnerId
        {
            get { return GetProperty<decimal?>(PARTNERIDPropertyName); }
            set { SetProperty(PARTNERIDPropertyName, value); }
        }

        #endregion .  Properties  .
    }
}