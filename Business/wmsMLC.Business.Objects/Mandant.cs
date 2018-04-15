namespace wmsMLC.Business.Objects
{
    public class Mandant : WMSBusinessObject
    {
        public const string ADDRESSPropertyName = "ADDRESS";
        public const string GLOBALPARAMVALPropertyName = "GLOBALPARAMVAL";
        public const string MANDANTBIKPropertyName = "MANDANTBIK";
        public const string MANDANTCODEPropertyName = "MANDANTCODE";
        public const string MANDANTCONTRACTPropertyName = "MANDANTCONTRACT";
        public const string MANDANTCORRESPONDENTACCOUNTPropertyName = "MANDANTCORRESPONDENTACCOUNT";
        public const string MANDANTDATECONTRACTPropertyName = "MANDANTDATECONTRACT";
        public const string MANDANTEMAILPropertyName = "MANDANTEMAIL";
        public const string MANDANTFAXPropertyName = "MANDANTFAX";
        public const string MANDANTFULLNAMEPropertyName = "MANDANTFULLNAME";
        public const string MANDANTHOSTREFPropertyName = "MANDANTHOSTREF";
        public const string MANDANTIDPropertyName = "MANDANTID";
        public const string MANDANTINNPropertyName = "MANDANTINN";
        public const string MANDANTKPPPropertyName = "MANDANTKPP";
        public const string MANDANTLOCKEDPropertyName = "MANDANTLOCKED";
        public const string MANDANTNAMEPropertyName = "MANDANTNAME";
        public const string MANDANTOGRNPropertyName = "MANDANTOGRN";
        public const string MANDANTOKPOPropertyName = "MANDANTOKPO";
        public const string MANDANTOKVEDPropertyName = "MANDANTOKVED";
        public const string MANDANTPHONEPropertyName = "MANDANTPHONE";
        public const string MANDANTSETTLEMENTACCOUNTPropertyName = "MANDANTSETTLEMENTACCOUNT";


        #region .  Properties  .
        public decimal? MandantId
        {
            get { return GetProperty<decimal?>(MANDANTIDPropertyName); }
            set { SetProperty(MANDANTIDPropertyName, value); }
        }

        public string MandantCode
        {
            get { return GetProperty<string>(MANDANTCODEPropertyName); }
            set { SetProperty(MANDANTCODEPropertyName, value); }
        }

        public string MandantName
        {
            get { return GetProperty<string>(MANDANTNAMEPropertyName); }
            set { SetProperty(MANDANTNAMEPropertyName, value); }
        }

        public WMSBusinessCollection<AddressBook> Address
        {
            get { return GetProperty<WMSBusinessCollection<AddressBook>>(ADDRESSPropertyName); }
            set { SetProperty(ADDRESSPropertyName, value); }
        }

        public WMSBusinessCollection<MandantGpv> GlobalParamVal
        {
            get { return GetProperty<WMSBusinessCollection<MandantGpv>>(GLOBALPARAMVALPropertyName); }
            set { SetProperty(GLOBALPARAMVALPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}
