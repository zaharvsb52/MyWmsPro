using System;

namespace wmsMLC.Business.Objects
{
    public class IWB : WMSBusinessObject
    {
        #region .  Constants  .
        public const string CUSTOMPARAMVALPropertyName = "CUSTOMPARAMVAL";
        public const string IWB2CARGOLPropertyName = "IWB2CARGOL";
        public const string IWBHOSTREFPropertyName = "IWBHOSTREF";
        public const string IWBIDPropertyName = "IWBID";
        public const string IWBNAMEPropertyName = "IWBNAME";
        public const string IWBPAYERPropertyName = "IWBPAYER";
        public const string IWBPAYER_NAMEPropertyName = "IWBPAYER_NAME";
        public const string IWBPOSLPropertyName = "IWBPOSL";
        public const string IWBPRIORITYPropertyName = "IWBPRIORITY";
        public const string IWBRECIPIENTPropertyName = "IWBRECIPIENT";
        public const string IWBRECIPIENT_NAMEPropertyName = "IWBRECIPIENT_NAME";
        public const string IWBSENDERPropertyName = "IWBSENDER";
        public const string IWBSENDER_NAMEPropertyName = "IWBSENDER_NAME";
        public const string MANDANTIDPropertyName = "MANDANTID";
        public const string OWBID_RPropertyName = "OWBID_R";
        public const string STATUSCODE_RPropertyName = "STATUSCODE_R";
        public const string STATUSCODE_R_NAMEPropertyName = "STATUSCODE_R_NAME";
        public const string IWBINVOICEPropertyName = "IWBINVOICENUMBER";
        public const string FACTORYID_RPropertyName = "FACTORYID_R";
        public const string VMANDANTCODEPropertyName = "VMANDANTCODE";
        public const string IWBCMRPropertyName = "IWBCMRNUMBER";
        public const string IWBDescPropertyName = "IWBDESC";
        public const string IWBDATEINSPropertyName = "DATEINS";
        public const string VTRAFFICARRIVEDPropertyName = "VINTERNALTRAFFICFACTARRIVED";
        public const string VTRAFFICDEPARTEDPropertyName = "VINTERNALTRAFFICFACTDEPARTED";
        public const string VLOADBEGINPropertyName = "VCARGOIWBLOADBEGIN";
        public const string VLOADENDPropertyName = "VCARGOIWBLOADEND";
        public const string IWBINDATEPLANPropertyName = "IWBINDATEPLAN";
        public const string IWBTYPEPropertyName = "IWBTYPE";
        public const string INVID_RPropertyName = "INVID_R";
        #endregion .  Constants  .

        #region .  Properties  .
        public string StatusCode
        {
            get { return GetProperty<string>(STATUSCODE_RPropertyName); }
            set { SetProperty(STATUSCODE_RPropertyName, value); }
        }
        public WMSBusinessCollection<IWBCpv> CustomParamVal
        {
            get { return GetProperty<WMSBusinessCollection<IWBCpv>>(CUSTOMPARAMVALPropertyName); }
            set { SetProperty(CUSTOMPARAMVALPropertyName, value); }
        }
        public WMSBusinessCollection<IWBPos> IWBPosL
        {
            get { return GetProperty<WMSBusinessCollection<IWBPos>>(IWBPOSLPropertyName); }
            set { SetProperty(IWBPOSLPropertyName, value); }
        }
        public WMSBusinessCollection<IWB2Cargo> IWB2CargoL
        {
            get { return GetProperty<WMSBusinessCollection<IWB2Cargo>>(IWB2CARGOLPropertyName); }
            set { SetProperty(IWB2CARGOLPropertyName, value); }
        }
        public decimal? MandantID
        {
            get { return GetProperty<decimal?>(MANDANTIDPropertyName); }
            set { SetProperty(MANDANTIDPropertyName, value); }
        }
        public decimal? FactoryID_R
        {
            get { return GetProperty<decimal?>(FACTORYID_RPropertyName); }
            set { SetProperty(FACTORYID_RPropertyName, value); }
        }
        public string IWBName
        {
            get { return GetProperty<string>(IWBNAMEPropertyName); }
            set { SetProperty(IWBNAMEPropertyName, value); }
        }
        public decimal? IWBID
        {
            get { return GetProperty<decimal?>(IWBIDPropertyName); }
            set { SetProperty(IWBIDPropertyName, value); }
        }
        public string IWBDesc
        {
            get { return GetProperty<string>(IWBDescPropertyName); }
            set { SetProperty(IWBDescPropertyName, value); }
        }
        public DateTime? DateIns
        {
            get { return GetProperty<DateTime?>(IWBDATEINSPropertyName); }
            set { SetProperty(IWBDATEINSPropertyName, value); }
        }
        public string StatusName
        {
            get { return GetProperty<string>(STATUSCODE_R_NAMEPropertyName); }
            set { SetProperty(STATUSCODE_R_NAMEPropertyName, value); }
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
        public string IWBHostRef
        {
            get { return GetProperty<string>(IWBHOSTREFPropertyName); }
            set { SetProperty(IWBHOSTREFPropertyName, value); }
        }
        public DateTime? InDatePlan
        {
            get { return GetProperty<DateTime?>(IWBINDATEPLANPropertyName); }
            set { SetProperty(IWBINDATEPLANPropertyName, value); }
        }
        public string IWBType
        {
            get { return GetProperty<string>(IWBTYPEPropertyName); }
            set { SetProperty(IWBTYPEPropertyName, value); }
        }
        public string SenderName
        {
            get { return GetProperty<string>(IWBSENDER_NAMEPropertyName); }
            set { SetProperty(IWBSENDER_NAMEPropertyName, value); }
        }

        public decimal? OWBId
        {
            get { return GetProperty<decimal?>(OWBID_RPropertyName); }
            set { SetProperty(OWBID_RPropertyName, value); }
        }
        public decimal? INVId
        {
            get { return GetProperty<decimal?>(INVID_RPropertyName); }
            set { SetProperty(INVID_RPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}