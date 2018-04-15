namespace wmsMLC.Business.Objects
{
    public class BillContract : WMSBusinessObject
    {
        #region . Constants .
        public const string BILLOPERATION2CONTRACTLPropertyName = "BILLOPERATION2CONTRACTL";
        public const string CONTRACTCUSTOMERPropertyName = "CONTRACTCUSTOMER";
        public const string CONTRACTDATEFROMPropertyName = "CONTRACTDATEFROM";
        public const string CONTRACTDATETILLPropertyName = "CONTRACTDATETILL";
        public const string CONTRACTDESCPropertyName = "CONTRACTDESC";
        public const string CONTRACTHOSTREFPropertyName = "CONTRACTHOSTREF";
        public const string CONTRACTIDPropertyName = "CONTRACTID";
        public const string CONTRACTNUMBERPropertyName = "CONTRACTNUMBER";
        public const string CONTRACTOWNERPropertyName = "CONTRACTOWNER";
        public const string CURRENCYCODE_RPropertyName = "CURRENCYCODE_R";
        public const string VATTYPECODE_RPropertyName = "VATTYPECODE_R";
        #endregion

        #region . Properties .
        public WMSBusinessCollection<BillOperation2Contract> BillOperation2ContractL
        {
            get { return GetProperty<WMSBusinessCollection<BillOperation2Contract>>(BILLOPERATION2CONTRACTLPropertyName); }
            set { SetProperty(BILLOPERATION2CONTRACTLPropertyName, value); }
        }

        public decimal? CONTRACTID
        {
            get { return GetProperty<decimal?>(CONTRACTIDPropertyName); }
            set { SetProperty(CONTRACTIDPropertyName, value); }
        }

        public decimal? CONTRACTOWNER
        {
            get { return GetProperty<decimal?>(CONTRACTOWNERPropertyName); }
            set { SetProperty(CONTRACTOWNERPropertyName, value); }
        }

        public decimal? CONTRACTCUSTOMER
        {
            get { return GetProperty<decimal?>(CONTRACTCUSTOMERPropertyName); }
            set { SetProperty(CONTRACTCUSTOMERPropertyName, value); }
        }
        #endregion
    }
}