using System;

namespace wmsMLC.Business.Objects
{
    public class OWB : WMSBusinessObject
    {
        #region .  Constants  .
        public const string ADDRESSBOOKID_RPropertyName = "ADDRESSBOOKID_R";
        public const string MANDANTIDPropertyName = "MANDANTID";
        public const string OWB2CARGOLPropertyName = "OWB2CARGOL";
        public const string OWBHOSTREFPropertyName = "OWBHOSTREF";
        public const string OWBIDPropertyName = "OWBID";
        public const string StatusCodePropertyName = "STATUSCODE_R";
        public const string OWBNAMEPropertyName = "OWBNAME";
        public const string OWBOUTDATEPLANPropertyName = "OWBOUTDATEPLAN";
        public const string OWBPOSLPropertyName = "OWBPOSL";
        public const string OWBPRIORITYPropertyName = "OWBPRIORITY";
        public const string OWBPRODUCTNEEDPropertyName = "OWBPRODUCTNEED";
        public const string OWBRECIPIENTPropertyName = "OWBRECIPIENT";
        public const string OWBRESERVTYPEPropertyName = "OWBRESERVTYPE";
        public const string STATUSCODE_RPropertyName = "STATUSCODE_R";
        public const string CUSTOMPARAMVALPropertyName = "CUSTOMPARAMVAL";
        public const string FACTORYID_RPropertyName = "FACTORYID_R";
        public const string VEXTERNALTRAFFICPropertyName = "VEXTERNALTRAFFIC";
        public const string VEXTERNALTRAFFICDRIVERFIOPropertyName = "VEXTERNALTRAFFICDRIVERFIO";
        public const string OWBPROXYDATEPropertyName = "OWBPROXYDATE";
        public const string OWBPROXYNUMBERPropertyName = "OWBPROXYNUMBER";
        public const string OWBOWNERPropertyName = "OWBOWNER";
        public const string OWBGROUPPropertyName = "OWBGROUP";
        public const string OWBHOSTREFDATEPropertyName = "OWBHOSTREFDATE";
        public const string OWBDATEINSPropertyName = "DATEINS";
        public const string STATUSCODE_R_NAMEPropertyName = "VSTATUSCODENAME";
        public const string VTRAFFICARRIVEDPropertyName = "VINTERNALTRAFFICFACTARRIVED";
        public const string VTRAFFICDEPARTEDPropertyName = "VINTERNALTRAFFICFACTDEPARTED";
        public const string VLOADBEGINPropertyName = "VCARGOOWBLOADBEGIN";
        public const string VLOADENDPropertyName = "VCARGOOWBLOADEND";
        public const string OWBTYPEPropertyName = "OWBTYPE";
        public const string OWBRECIPIENT_NAMEPropertyName = "VOWBRECIPIENTNAME";
        public const string OWBDescPropertyName = "OWBDESC";
        public const string VAddressPropertyName = "VADDRESSBOOKCOMPLEX";
        public const string EMPLOYEE2OWBLPropertyName = "EMPLOYEE2OWBL";
        public const string OWBClientRecipientPropertyName = "OWBCLIENTRECIPIENT";
        public const string OWBClientPayerPropertyName = "OWBCLIENTPAYER";
        public const string OWBClientRecipientAddrPropertyName = "OWBCLIENTRECIPIENTADDR";
        #endregion .  Constants  .

        #region .  Properties  .

        public decimal OWBID
        {
            get { return GetProperty<decimal>(OWBIDPropertyName); }
            set { SetProperty(OWBIDPropertyName, value); }
        }

        public string StatusCode
        {
            get { return GetProperty<string>(StatusCodePropertyName); }
            set { SetProperty(StatusCodePropertyName,value); }
        }

        public decimal? OWBRecipient
        {
            get { return GetProperty<decimal>(OWBRECIPIENTPropertyName); }
            set { SetProperty(OWBRECIPIENTPropertyName, value); }
        }

        public decimal? AddressBookID
        {
            get { return GetProperty<decimal>(ADDRESSBOOKID_RPropertyName); }
            set { SetProperty(ADDRESSBOOKID_RPropertyName, value); }
        }

        public WMSBusinessCollection<OWBPos> OWBPosL
        {
            get { return GetProperty<WMSBusinessCollection<OWBPos>>(OWBPOSLPropertyName); }
            set { SetProperty(OWBPOSLPropertyName, value); }
        }

        public decimal? MandantID
        {
            get { return GetProperty<decimal>(MANDANTIDPropertyName); }
            set { SetProperty(MANDANTIDPropertyName, value); }
        }

        public WMSBusinessCollection<OWBCpv> CustomParamVal
        {
            get { return GetProperty<WMSBusinessCollection<OWBCpv>>(CUSTOMPARAMVALPropertyName); }
            set { SetProperty(CUSTOMPARAMVALPropertyName, value); }
        }

        public decimal? FactoryID_R
        {
            get { return GetProperty<decimal?>(FACTORYID_RPropertyName); }
            set { SetProperty(FACTORYID_RPropertyName, value); }
        }

        public decimal? Owner
        {
            get { return GetProperty<decimal>(OWBOWNERPropertyName); }
            set { SetProperty(OWBOWNERPropertyName, value); }
        }
        public string Group
        {
            get { return GetProperty<string>(OWBGROUPPropertyName); }
            set { SetProperty(OWBGROUPPropertyName, value); }
        }
        public DateTime? DateIns
        {
            get { return GetProperty<DateTime?>(OWBDATEINSPropertyName); }
            set { SetProperty(OWBDATEINSPropertyName, value); }
        }
        public string TrafficArrived
        {
            get { return GetProperty<string>(VTRAFFICARRIVEDPropertyName); }
            set { SetProperty(VTRAFFICARRIVEDPropertyName, value); }
        }
        public string TrafficDeparted
        {
            get { return GetProperty<string>(VTRAFFICDEPARTEDPropertyName); }
            set { SetProperty(VTRAFFICDEPARTEDPropertyName, value); }
        }
        public string LoadBegin
        {
            get { return GetProperty<string>(VLOADBEGINPropertyName); }
            set { SetProperty(VLOADBEGINPropertyName, value); }
        }
        public string LoadEnd
        {
            get { return GetProperty<string>(VLOADENDPropertyName); }
            set { SetProperty(VLOADENDPropertyName, value); }
        }
        public DateTime? OutDatePlan
        {
            get { return GetProperty<DateTime?>(OWBOUTDATEPLANPropertyName); }
            set { SetProperty(OWBOUTDATEPLANPropertyName, value); }
        }
        public string OWBType
        {
            get { return GetProperty<string>(OWBTYPEPropertyName); }
            set { SetProperty(OWBTYPEPropertyName, value); }
        }
        public string RecipientName
        {
            get { return GetProperty<string>(OWBRECIPIENT_NAMEPropertyName); }
            set { SetProperty(OWBRECIPIENT_NAMEPropertyName, value); }
        }
        public string OWBHostRef
        {
            get { return GetProperty<string>(OWBHOSTREFPropertyName); }
            set { SetProperty(OWBHOSTREFPropertyName, value); }
        }
        public string OWBName
        {
            get { return GetProperty<string>(OWBNAMEPropertyName); }
            set { SetProperty(OWBNAMEPropertyName, value); }
        }
        public string OWBDesc
        {
            get { return GetProperty<string>(OWBDescPropertyName); }
            set { SetProperty(OWBDescPropertyName, value); }
        }
        public string StatusName
        {
            get { return GetProperty<string>(STATUSCODE_R_NAMEPropertyName); }
            set { SetProperty(STATUSCODE_R_NAMEPropertyName, value); }
        }
        public string AddressBookRecipient
        {
            get { return GetProperty<string>(VAddressPropertyName); }
            set { SetProperty(VAddressPropertyName, value); }
        }

        public WMSBusinessCollection<Employee2OWB> Employee2OwbL
        {
            get { return GetProperty<WMSBusinessCollection<Employee2OWB>>(EMPLOYEE2OWBLPropertyName); }
            set { SetProperty(EMPLOYEE2OWBLPropertyName, value); }
        }

        public decimal? OWBClientRecipient
        {
            get { return GetProperty<decimal>(OWBClientRecipientPropertyName); }
            set { SetProperty(OWBClientRecipientPropertyName, value); }
        }

        public decimal? OWBClientPayer
        {
            get { return GetProperty<decimal>(OWBClientPayerPropertyName); }
            set { SetProperty(OWBClientPayerPropertyName, value); }
        }

        public decimal? OWBClientRecipientAddr
        {
            get { return GetProperty<decimal?>(OWBClientRecipientAddrPropertyName); }
            set { SetProperty(OWBClientRecipientAddrPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}