namespace wmsMLC.Business.Objects
{
    public class BillOperationCause : WMSBusinessObject
    {
        #region .  Constants  .
        public const string OperationCauseNamePropertyName = "OPERATIONCAUSENAME";
        public const string OperationCauseOrdinalPropertyName = "OPERATIONCAUSEORDINAL";
        public const string OperationCauseCpvLPropertyName = "OPERATIONCAUSECPVL";
        #endregion .  Constants  .

        #region .  Properties  .
        public string OperationCauseName
        {
            get { return GetProperty<string>(OperationCauseNamePropertyName); }
            set { SetProperty(OperationCauseNamePropertyName, value); }
        }

        public decimal OperationCauseOrdinal
        {
            get { return GetProperty<decimal>(OperationCauseOrdinalPropertyName); }
            set { SetProperty(OperationCauseOrdinalPropertyName, value); }
        }

        public WMSBusinessCollection<BillOperationCauseCpv> OperationCauseCpvL
        {
            get { return GetProperty<WMSBusinessCollection<BillOperationCauseCpv>>(OperationCauseCpvLPropertyName); }
            set { SetProperty(OperationCauseCpvLPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}