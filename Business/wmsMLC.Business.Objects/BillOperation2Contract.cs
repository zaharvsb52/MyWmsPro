namespace wmsMLC.Business.Objects
{
    public class BillOperation2Contract : WMSBusinessObject
    {
        #region . Constants .
        public const string BILLOPERATION2CONTRACTANALYTICSCODEPropertyName = "BILLOPERATION2CONTRACTANALYTICSCODE";
        public const string BILLOPERATION2CONTRACTBILLERCODEPropertyName = "BILLOPERATION2CONTRACTBILLERCODE";
        public const string BILLOPERATION2CONTRACTCONTRACTIDPropertyName = "BILLOPERATION2CONTRACTCONTRACTID";
        public const string BILLOPERATION2CONTRACTOPERATIONCODEPropertyName = "BILLOPERATION2CONTRACTOPERATIONCODE";
        public const string BILLOPERATIONCAUSELPropertyName = "BILLOPERATIONCAUSEL";
        public const string BILLSCALE2O2CLPropertyName = "BILLSCALE2O2CL";
        public const string BILLTARIFFLPropertyName = "BILLTARIFFL";
        public const string OPERATION2CONTRACTDESCPropertyName = "OPERATION2CONTRACTDESC";
        public const string OPERATION2CONTRACTIDPropertyName = "OPERATION2CONTRACTID";
        public const string OPERATION2CONTRACTNAMEPropertyName = "OPERATION2CONTRACTNAME";
        public const string Operation2ContractCpvLPropertyName = "OPERATION2CONTRACTCPVL";

        #endregion . Constants .

        #region . Properties .
        public WMSBusinessCollection<BillOperationCause> BillOperationCauseL
        {
            get { return GetProperty<WMSBusinessCollection<BillOperationCause>>(BILLOPERATIONCAUSELPropertyName); }
            set { SetProperty(BILLOPERATIONCAUSELPropertyName, value); }
        }

        public WMSBusinessCollection<BillScale2O2C> BillScale2O2CL
        {
            get { return GetProperty<WMSBusinessCollection<BillScale2O2C>>(BILLSCALE2O2CLPropertyName); }
            set { SetProperty(BILLSCALE2O2CLPropertyName, value); }
        }

        public WMSBusinessCollection<BillTariff> BillTariffL
        {
            get { return GetProperty<WMSBusinessCollection<BillTariff>>(BILLTARIFFLPropertyName); }
            set { SetProperty(BILLTARIFFLPropertyName, value); }
        }

        public string BILLOPERATION2CONTRACTANALYTICSCODE
        {
            get { return GetProperty<string>(BILLOPERATION2CONTRACTANALYTICSCODEPropertyName); }
            set { SetProperty(BILLOPERATION2CONTRACTANALYTICSCODEPropertyName, value); }
        }

        public WMSBusinessCollection<BillOperation2ContractCpv> Operation2ContractCpvL
        {
            get { return GetProperty<WMSBusinessCollection<BillOperation2ContractCpv>>(Operation2ContractCpvLPropertyName); }
            set { SetProperty(Operation2ContractCpvLPropertyName, value); }
        }

        public decimal Operation2ContractID
        {
            get { return GetProperty<decimal>(OPERATION2CONTRACTIDPropertyName); }
            set { SetProperty(OPERATION2CONTRACTIDPropertyName, value); }
        }

        #endregion . Properties .
    }
}