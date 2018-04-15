namespace wmsMLC.Business.Objects
{
    public class PartnerGroup : WMSBusinessObject
    {
        #region .  Constants  .

        public const string PARTNERGROUPIDPropertyName = "PARTNERGROUPID";
        public const string PARTNERGROUPNAMEPropertyName = "PARTNERGROUPNAME";
        public const string PARTNERGROUPHOSTREFPropertyName = "PARTNERGROUPHOSTREF";

        #endregion .  Constants  .

        #region .  Properties  .

        public decimal? PartnerGroupId
        {
            get { return GetProperty<decimal?>(PARTNERGROUPIDPropertyName); }
            set { SetProperty(PARTNERGROUPIDPropertyName, value); }
        }

        public string PartnerGroupName
        {
            get { return GetProperty<string>(PARTNERGROUPNAMEPropertyName); }
            set { SetProperty(PARTNERGROUPNAMEPropertyName, value); }
        }

        public string PartnerGroupHostRef
        {
            get { return GetProperty<string>(PARTNERGROUPHOSTREFPropertyName); }
            set { SetProperty(PARTNERGROUPHOSTREFPropertyName, value); }
        }

        #endregion .  Properties  .
    }
}