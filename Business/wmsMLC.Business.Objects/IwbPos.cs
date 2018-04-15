using System;

namespace wmsMLC.Business.Objects
{
    public class IWBPos : WMSBusinessObject
    {

        #region .  Constants  .
        public const string CUSTOMPARAMVALPropertyName = "CUSTOMPARAMVAL";
        public const string IWBPosCountPropertyName = "IWBPOSCOUNT";
        public const string IWBPosCount2SKUPropertyName = "IWBPOSCOUNT2SKU";
        public const string SKUIDPropertyName = "SKUID_R";
        public const string IWBPosColorPropertyName = "IWBPOSCOLOR";
        public const string IWBPosTonePropertyName = "IWBPOSTONE";
        public const string IWBPosSizePropertyName = "IWBPOSSIZE";
        public const string IWBPosBatchPropertyName = "IWBPOSBATCH";
        public const string IWBPosExpiryDatePropertyName = "IWBPOSEXPIRYDATE";
        public const string IWBPosProductDatePropertyName = "IWBPOSPRODUCTDATE";
        public const string IWBPosSerialNumberPropertyName = "IWBPOSSERIALNUMBER";
        public const string QLFCode_rPropertyName = "QLFCODE_R";
        public const string IWBPosBlockingPropertyName = "IWBPOSBLOCKING";
        public const string IWBPosHostRefPropertyName = "IWBPOSHOSTREF";
        public const string IWBID_RPropertyName = "IWBID_R";
        public const string MANDANTIDPropertyName = "MANDANTID";
        public const string VARTNAMEPropertyName = "VARTNAME";
        public const string VARTDESCPropertyName = "VARTDESC";
        public const string VMEASURENAMEPropertyName = "VMEASURENAME";
        public const string FACTORYID_RPropertyName = "FACTORYID_R";
        public const string IWBPosLotPropertyName = "IWBPOSLOT";
        public const string StatusCodePropertyName = "STATUSCODE_R";
        public const string IWBPosArtNamePropertyName = "IWBPOSARTNAME";
        public const string IWBPosNumberPropertyName = "IWBPOSNUMBER";
        public const string IWBPosInvoiceNumberPropertyName = "IWBPOSINVOICENUMBER";
        public const string IWBPosInvoiceDatePropertyName = "IWBPOSINVOICEDATE";
        public const string IWBPosIDPropertyName = "IWBPOSID";
        public const string IWBBatchCodePropertyName = "IWBBATCHCODE";
        public const string IWBPosBoxNumberPropertyName = "IWBPOSBOXNUMBER";
        public const string IWBPosProductCountPropertyName = "IWBPOSPRODUCTCOUNT";
        public const string IWBPosOwnerPropertyName = "IWBPOSOWNER";
        public const string VQLFNAMEPropertyName = "VQLFNAME";
        public const string VSkuNamePropertyName = "SKUID_r_Name";
        #endregion

        #region .  Properties  .
        public WMSBusinessCollection<IWBPosCpv> CustomParamVal
        {
            get { return GetProperty<WMSBusinessCollection<IWBPosCpv>>(CUSTOMPARAMVALPropertyName); }
            set { SetProperty(CUSTOMPARAMVALPropertyName, value); }
        }

        public string StatusCode
        {
            get { return GetProperty<string>(StatusCodePropertyName); }
            set { SetProperty(StatusCodePropertyName, value); }
        }

        public decimal IWBPosCount
        {
            get { return GetProperty<decimal>(IWBPosCountPropertyName); }
            set { SetProperty(IWBPosCountPropertyName, value); }
        }

        public double IWBPosProductCount
        {
            get { return GetProperty<double>(IWBPosProductCountPropertyName); }
            set { SetProperty(IWBPosProductCountPropertyName, value); }
        }

        public double IWBPosCount2SKU
        {
            get { return GetProperty<double>(IWBPosCount2SKUPropertyName); }
            set { SetProperty(IWBPosCount2SKUPropertyName, value); }
        }

        public decimal SKUID
        {
            get { return GetProperty<decimal>(SKUIDPropertyName); }
            set { SetProperty(SKUIDPropertyName, value); }
        }

        public string IWBPosColor
        {
            get { return GetProperty<string>(IWBPosColorPropertyName); }
            set { SetProperty(IWBPosColorPropertyName, value); }
        }

        public string IWBPosTone
        {
            get { return GetProperty<string>(IWBPosTonePropertyName); }
            set { SetProperty(IWBPosTonePropertyName, value); }
        }

        public string IWBPosSize
        {
            get { return GetProperty<string>(IWBPosSizePropertyName); }
            set { SetProperty(IWBPosSizePropertyName, value); }
        }

        public string IWBPosBatch
        {
            get { return GetProperty<string>(IWBPosBatchPropertyName); }
            set { SetProperty(IWBPosBatchPropertyName, value); }
        }

        public DateTime? IWBPosExpiryDate
        {
            get { return GetProperty<DateTime?>(IWBPosExpiryDatePropertyName); }
            set { SetProperty(IWBPosExpiryDatePropertyName, value); }
        }

        public DateTime? IWBPosProductDate
        {
            get { return GetProperty<DateTime?>(IWBPosProductDatePropertyName); }
            set { SetProperty(IWBPosProductDatePropertyName, value); }
        }

        public string IWBPosSerialNumber
        {
            get { return GetProperty<string>(IWBPosSerialNumberPropertyName); }
            set { SetProperty(IWBPosSerialNumberPropertyName, value); }
        }

        public string QLFCode_r
        {
            get { return GetProperty<string>(QLFCode_rPropertyName); }
            set { SetProperty(QLFCode_rPropertyName, value); }
        }

        public string IWBPosBlocking
        {
            get { return GetProperty<string>(IWBPosBlockingPropertyName); }
            set { SetProperty(IWBPosBlockingPropertyName, value); }
        }

        public string IWBPosHostRef
        {
            get { return GetProperty<string>(IWBPosHostRefPropertyName); }
            set { SetProperty(IWBPosHostRefPropertyName, value); }
        }

        public decimal? IWBID_R
        {
            get { return GetProperty<decimal>(IWBID_RPropertyName); }
            set { SetProperty(IWBID_RPropertyName, value); }
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

        public decimal? FactoryID_R
        {
            get { return GetProperty<decimal?>(FACTORYID_RPropertyName); }
            set { SetProperty(FACTORYID_RPropertyName, value); }
        }

        public string IWBPosLot
        {
            get { return GetProperty<string>(IWBPosLotPropertyName); }
            set { SetProperty(IWBPosLotPropertyName, value); }
        }

        public string IWBPosArtName
        {
            get { return GetProperty<string>(IWBPosArtNamePropertyName); }
            set { SetProperty(IWBPosArtNamePropertyName, value); }
        }

        public decimal IWBPosNumber
        {
            get { return GetProperty<decimal>(IWBPosNumberPropertyName); }
            set { SetProperty(IWBPosNumberPropertyName, value); }
        }

        public string IWBPosInvoiceNumber
        {
            get { return GetProperty<string>(IWBPosInvoiceNumberPropertyName); }
            set { SetProperty(IWBPosInvoiceNumberPropertyName, value); }
        }

        public DateTime? IWBPosInvoiceDate
        {
            get { return GetProperty<DateTime?>(IWBPosInvoiceDatePropertyName); }
            set { SetProperty(IWBPosInvoiceDatePropertyName, value); }
        }

        public decimal? IWBPosID
        {
            get { return GetProperty<decimal>(IWBPosIDPropertyName); }
            set { SetProperty(IWBPosIDPropertyName, value); }
        }

        public string IWBBatchCode
        {
            get { return GetProperty<string>(IWBBatchCodePropertyName); }
            set { SetProperty(IWBBatchCodePropertyName, value); }
        }

        public string IWBPosBoxNumber
        {
            get { return GetProperty<string>(IWBPosBoxNumberPropertyName); }
            set { SetProperty(IWBPosBoxNumberPropertyName, value); }
        }

        public decimal? IWBPosOwner
        {
            get { return GetProperty<decimal>(IWBPosOwnerPropertyName); }
            set { SetProperty(IWBPosOwnerPropertyName, value); }
        }

        public string VQlfName
        {
            get { return GetProperty<string>(VQLFNAMEPropertyName); }
            set { SetProperty(VQLFNAMEPropertyName, value); }
        }
        
        public string VSkuName
        {
            get { return GetProperty<string>(VSkuNamePropertyName); }
            set { SetProperty(VSkuNamePropertyName, value); }
        }
       
        #endregion
    }
}