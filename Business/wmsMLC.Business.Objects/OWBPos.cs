using System;

namespace wmsMLC.Business.Objects
{
    public class OWBPos : WMSBusinessObject
    {
        #region .  Constants  .

        public const string OWBID_RPropertyName = "OWBID_R";
        public const string MANDANTIDPropertyName = "MANDANTID";
        public const string VARTNAMEPropertyName = "VARTNAME";
        public const string VARTDESCPropertyName = "VARTDESC";
        public const string VMEASURENAMEPropertyName = "VMEASURENAME";
        public const string SKUIDPropertyName = "SKUID_R";
        public const string FACTORYID_RPropertyName = "FACTORYID_R";
        public const string OWBPOSLOTPropertyName = "OWBPOSLOT";
        public const string OWBPosArtNamePropertyName = "OWBPOSARTNAME";
        public const string OWBPosNumberPropertyName = "OWBPOSNUMBER";
        public const string OWBPosCountPropertyName = "OWBPOSCOUNT";
        public const string OWBPosCount2SKUPropertyName = "OWBPOSCOUNT2SKU";
        public const string OWBPosIDPropertyName = "OWBPOSID";
        public const string OWBPosOwnerPropertyName = "OWBPOSOWNER";
        public const string VQLFNAMEPropertyName = "VQLFNAME";
        public const string OWBPosReservedPropertyName = "OWBPOSRESERVED";
        public const string OWBPosWantagePropertyName = "OWBPOSWANTAGE";
        public const string OWBPosBatchPropertyName = "OWBPOSBATCH";
        public const string STATUSCODE_RPropertyName = "STATUSCODE_R";
        public const string OWBPOSKITARTNAMEPropertyName = "OWBPOSKITARTNAME";
        public const string QLFCODE_RPropertyName = "QLFCODE_R";
        public const string QLFDETAILCODE_RPropertyName = "QLFDETAILCODE_R";
        public const string OWBPOSEXPIRYDATEPropertyName = "OWBPOSEXPIRYDATE";
        public const string OWBPOSCOLORPropertyName = "OWBPOSCOLOR";
        public const string OWBPOSSIZEPropertyName = "OWBPOSSIZE";
        public const string OWBPOSPRODUCTDATEPropertyName = "OWBPOSPRODUCTDATE";
        public const string OWBPOSSERIALNUMBERPropertyName = "OWBPOSSERIALNUMBER";
        public const string OWBPOSTONEPropertyName = "OWBPOSTONE";
        public const string OWBPOSBOXNUMBERPropertyName = "OWBPOSBOXNUMBER";
        public const string CUSTOMPARAMVALPropertyName = "CUSTOMPARAMVAL";
        #endregion .  Constants  .

        #region .  Properties  .

        public decimal? OWBID_R
        {
            get { return GetProperty<decimal>(OWBID_RPropertyName); }
            set { SetProperty(OWBID_RPropertyName, value); }
        }

        public decimal? MandantID
        {
            get { return GetProperty<decimal>(MANDANTIDPropertyName); }
            set { SetProperty(MANDANTIDPropertyName, value); }
        }

        public string VArtName
        {
            get { return GetProperty<string>(VARTNAMEPropertyName); }
            set { SetProperty(VARTNAMEPropertyName, value); }
        }

        public string VArtDesc
        {
            get { return GetProperty<string>(VARTDESCPropertyName); }
            set { SetProperty(VARTDESCPropertyName, value); }
        }

        public string VMeasureName
        {
            get { return GetProperty<string>(VMEASURENAMEPropertyName); }
            set { SetProperty(VMEASURENAMEPropertyName, value); }
        }

        public decimal? SKUID
        {
            get { return GetProperty<decimal?>(SKUIDPropertyName); }
            set { SetProperty(SKUIDPropertyName, value); }
        }

        public decimal? FactoryID_R
        {
            get { return GetProperty<decimal?>(FACTORYID_RPropertyName); }
            set { SetProperty(FACTORYID_RPropertyName, value); }
        }

        public string OWBPOSLOT
        {
            get { return GetProperty<string>(OWBPOSLOTPropertyName); }
            set { SetProperty(OWBPOSLOTPropertyName, value); }
        }

        public string OWBPosArtName
        {
            get { return GetProperty<string>(OWBPosArtNamePropertyName); }
            set { SetProperty(OWBPosArtNamePropertyName, value); }
        }

        public decimal OWBPosNumber
        {
            get { return GetProperty<decimal>(OWBPosNumberPropertyName); }
            set { SetProperty(OWBPosNumberPropertyName, value); }
        }

        public decimal OWBPosCount
        {
            get { return GetProperty<decimal>(OWBPosCountPropertyName); }
            set { SetProperty(OWBPosCountPropertyName, value); }
        }

        public double OWBPosCount2SKU
        {
            get { return GetProperty<double>(OWBPosCount2SKUPropertyName); }
            set { SetProperty(OWBPosCount2SKUPropertyName, value); }
        }

        public decimal? OWBPosID
        {
            get { return GetProperty<decimal>(OWBPosIDPropertyName); }
            set { SetProperty(OWBPosIDPropertyName, value); }
        }

        public decimal? OWBPosOwner
        {
            get { return GetProperty<decimal>(OWBPosOwnerPropertyName); }
            set { SetProperty(OWBPosOwnerPropertyName, value); }
        }
        public decimal? OWBPosReserved
        {
            get { return GetProperty<decimal>(OWBPosReservedPropertyName); }
            set { SetProperty(OWBPosReservedPropertyName, value); }
        }
        public decimal? OWBPosWantage
        {
            get { return GetProperty<decimal>(OWBPosWantagePropertyName); }
            set { SetProperty(OWBPosWantagePropertyName, value); }
        }
        public string OWBPosBatch
        {
            get { return GetProperty<string>(OWBPosBatchPropertyName); }
            set { SetProperty(OWBPosBatchPropertyName, value); }
        }
        public string VQlfName
        {
            get { return GetProperty<string>(VQLFNAMEPropertyName); }
            set { SetProperty(VQLFNAMEPropertyName, value); }
        }

        public string STATUSCODE_R
        {
            get { return GetProperty<string>(STATUSCODE_RPropertyName); }
            set { SetProperty(STATUSCODE_RPropertyName, value); }
        }

        public OWBPosStates Status
        {
            get
            {
                var status = GetProperty<string>(STATUSCODE_RPropertyName);
                return (OWBPosStates)Enum.Parse(typeof(OWBPosStates), status);
            }
            set { SetProperty(STATUSCODE_RPropertyName, value.ToString()); }
        }

        public string OWBPOSKITARTNAME
        {
            get { return GetProperty<string>(OWBPOSKITARTNAMEPropertyName); }
            set { SetProperty(OWBPOSKITARTNAMEPropertyName, value); }
        }

        public string QLFCODE_R
        {
            get { return GetProperty<string>(QLFCODE_RPropertyName); }
            set { SetProperty(QLFCODE_RPropertyName, value); }
        }
        
        public string QLFDETAILCODE_R
        {
            get { return GetProperty<string>(QLFDETAILCODE_RPropertyName); }
            set { SetProperty(QLFDETAILCODE_RPropertyName, value); }
        }

        public DateTime? OWBPOSEXPIRYDATE
        {
            get { return GetProperty<DateTime?>(OWBPOSEXPIRYDATEPropertyName); }
            set { SetProperty(OWBPOSEXPIRYDATEPropertyName, value); }
        }

        public string OWBPOSCOLOR
        {
            get { return GetProperty<string>(OWBPOSCOLORPropertyName); }
            set { SetProperty(OWBPOSCOLORPropertyName, value); }
        }

        public string OWBPOSSIZE
        {
            get { return GetProperty<string>(OWBPOSSIZEPropertyName); }
            set { SetProperty(OWBPOSSIZEPropertyName, value); }
        }

        public DateTime? OWBPOSPRODUCTDATE
        {
            get { return GetProperty<DateTime?>(OWBPOSPRODUCTDATEPropertyName); }
            set { SetProperty(OWBPOSPRODUCTDATEPropertyName, value); }
        }

        public string OWBPOSSERIALNUMBER
        {
            get { return GetProperty<string>(OWBPOSSERIALNUMBERPropertyName); }
            set { SetProperty(OWBPOSSERIALNUMBERPropertyName, value); }
        }

        public string OWBPOSTONE
        {
            get { return GetProperty<string>(OWBPOSTONEPropertyName); }
            set { SetProperty(OWBPOSTONEPropertyName, value); }
        }

        public string OWBPOSBOXNUMBER
        {
            get { return GetProperty<string>(OWBPOSBOXNUMBERPropertyName); }
            set { SetProperty(OWBPOSBOXNUMBERPropertyName, value); }
        }

        public WMSBusinessCollection<OWBPosCpv> CustomParamVal
        {
            get { return GetProperty<WMSBusinessCollection<OWBPosCpv>>(CUSTOMPARAMVALPropertyName); }
            set { SetProperty(CUSTOMPARAMVALPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}