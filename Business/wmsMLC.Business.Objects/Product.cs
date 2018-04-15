using System;

namespace wmsMLC.Business.Objects
{
    /// <summary>
    /// Сущность 'Товар'.
    /// </summary>
    public class Product : WMSBusinessObject
    {
        #region .  Constants  .
        public const string EntityType = "WmsProduct";

        public const string IWBPosIDPropertyName = "IWBPOSID_R";
        public const string TECodePropertyName = "TECODE_R";
        public const string SKUIDPropertyName = "SKUID_R";
        public const string ProductCountSKUPropertyName = "PRODUCTCOUNTSKU";
        public const string ProductCountPropertyName = "PRODUCTCOUNT";
        public const string QLFCode_rPropertyName = "QLFCODE_R";
        public const string QLFDETAILCODE_RPropertyName = "QLFDETAILCODE_R";
        public const string ProductIdPropertyName = "PRODUCTID";
        public const string ProductInputDatePropertyName = "PRODUCTINPUTDATE";
        public const string ProductExpiryDatePropertyName = "PRODUCTEXPIRYDATE";
        public const string ProductBatchPropertyName = "PRODUCTBATCH";
        public const string ProductSerialNumberPropertyName = "PRODUCTSERIALNUMBER";
        public const string ProductColorPropertyName = "PRODUCTCOLOR";
        public const string ProductTonePropertyName = "PRODUCTTONE";
        public const string ProductSizePropertyName = "PRODUCTSIZE";
        public const string FACTORYID_RPropertyName = "FACTORYID_R";
        public const string ProductHostRefPropertyName = "PRODUCTHOSTREF";
        public const string ProductInputDateMethodPropertyName = "PRODUCTINPUTDATEMETHOD";
        public const string Product2BlockingPropertyName = "PRODUCT2BLOCK";
        public const string StatusCode_RPropertyName = "STATUSCODE_R";
        public const string ProductCountPackPropertyName = "PRODUCTPACKCOUNTSKU";
        public const string TRANSPORTTASKID_RPropertyName = "TRANSPORTTASKID_R";
        public const string VOWBIdPropertyName = "VOWBID";
        public const string ArtCode_RPropertyName = "ARTCODE_R";
        public const string VPlaceCodePropertyName = "VPLACECODE";
        public const string VPlaceNamePropertyName = "VPLACENAME";
        public const string VIWBNamePropertyName = "VIWBNAME";
        public const string OWBPosIDPropertyName = "OWBPOSID_R";
        public const string VOWBNamePropertyName = "VOWBNAME";
        public const string VArtNamePropertyName = "VARTNAME";
        public const string VArtDescPropertyName = "VARTDESC";
        public const string SKUNamePropertyName = "SKUID_R_NAME";
        public const string ProductBoxNumberPropertyName = "PRODUCTBOXNUMBER";
        public const string MandantIDPropertyName = "MANDANTID";
        public const string PRODUCTOWNERPropertyName = "PRODUCTOWNER";
        public const string PRODUCTKITARTNAMEPropertyName = "PRODUCTKITARTNAME";
        public const string PRODUCTDATEPropertyName = "PRODUCTDATE";
        public const string PRODUCTLOTPropertyName = "PRODUCTLOT";
        #endregion .  Constants  .

        #region .  Properties  .
        public decimal? ProductId
        {
            get { return GetProperty<decimal?>(ProductIdPropertyName); }
            set { SetProperty(ProductIdPropertyName, value); }
        }

        public decimal ProductCountPack
        {
            get { return GetProperty<decimal>(ProductCountPackPropertyName); }
            set { SetProperty(ProductCountPackPropertyName, value); }
        }

        public string StatusCode_R
        {
            get { return GetProperty<string>(StatusCode_RPropertyName); }
            set { SetProperty(StatusCode_RPropertyName, value); }
        }

        public ProductStates Status
        {
            get
            {
                var status = GetProperty<string>(StatusCode_RPropertyName);
                return (ProductStates) Enum.Parse(typeof (ProductStates), status);
            }
            set { SetProperty(StatusCode_RPropertyName, value.ToString()); }
        }

        public decimal IWBPosID
        {
            get { return GetProperty<decimal>(IWBPosIDPropertyName); }
            set { SetProperty(IWBPosIDPropertyName, value); }
        }

        public string TECode
        {
            get { return GetProperty<string>(TECodePropertyName); }
            set { SetProperty(TECodePropertyName, value); }
        }

        public decimal SKUID
        {
            get { return GetProperty<decimal>(SKUIDPropertyName); }
            set { SetProperty(SKUIDPropertyName, value); }
        }

        public double ProductCount
        {
            get { return GetProperty<double>(ProductCountPropertyName); }
            set { SetProperty(ProductCountPropertyName, value); }
        }

        public decimal ProductCountSKU
        {
            get { return GetProperty<decimal>(ProductCountSKUPropertyName); }
            set { SetProperty(ProductCountSKUPropertyName, value); }
        }

        public string QLFCode_r 
        {
            get { return GetProperty<string>(QLFCode_rPropertyName); }
            set { SetProperty(QLFCode_rPropertyName, value); }
        }

        public string QLFDETAILCODE_R 
        {
            get { return GetProperty<string>(QLFDETAILCODE_RPropertyName); }
            set { SetProperty(QLFDETAILCODE_RPropertyName, value); }
        }

        public DateTime? ProductInputDate
        {
            get { return GetProperty<DateTime>(ProductInputDatePropertyName); }
            set { SetProperty(ProductInputDatePropertyName, value); }
        }

        public DateTime? ProductExpiryDate
        {
            get { return GetProperty<DateTime>(ProductExpiryDatePropertyName); }
            set { SetProperty(ProductExpiryDatePropertyName, value); }
        }

        public string ProductBatch
        {
            get { return GetProperty<string>(ProductBatchPropertyName); }
            set { SetProperty(ProductBatchPropertyName, value); }
        }

        public string ProductSerialNumber
        {
            get { return GetProperty<string>(ProductSerialNumberPropertyName); }
            set { SetProperty(ProductSerialNumberPropertyName, value); }
        }

        public string ProductColor
        {
            get { return GetProperty<string>(ProductColorPropertyName); }
            set { SetProperty(ProductColorPropertyName, value); }
        }

        public string ProductBoxNumber
        {
            get { return GetProperty<string>(ProductBoxNumberPropertyName); }
            set { SetProperty(ProductBoxNumberPropertyName, value); }
        }


        public string ProductTone
        {
            get { return GetProperty<string>(ProductTonePropertyName); }
            set { SetProperty(ProductTonePropertyName, value); }
        }

        public string ProductSize
        {
            get { return GetProperty<string>(ProductSizePropertyName); }
            set { SetProperty(ProductSizePropertyName, value); }
        }

        public decimal? FactoryID_R
        {
            get { return GetProperty<decimal?>(FACTORYID_RPropertyName); }
            set { SetProperty(FACTORYID_RPropertyName, value); }
        }

        public string ProductHostRef
        {
            get { return GetProperty<string>(ProductHostRefPropertyName); }
            set { SetProperty(ProductHostRefPropertyName, value); }
        }

        public string ProductInputDateMethod
        {
            get { return GetProperty<string>(ProductInputDateMethodPropertyName); }
            set { SetProperty(ProductInputDateMethodPropertyName, value); }
        }

        public WMSBusinessCollection<Product2Blocking> Product2Block
        {
            get { return GetProperty<WMSBusinessCollection<Product2Blocking>>(Product2BlockingPropertyName); }
            set { SetProperty(Product2BlockingPropertyName, value); }
        }

        public string VPlaceCode
        {
            get
            {
                return GetProperty<string>(VPlaceCodePropertyName);
            }
        }

        public string VPlaceName 
        {
            get
            {
                return GetProperty<string>(VPlaceNamePropertyName);
            }
        }

        public decimal? VOWBId
        {
            get
            {
                return GetProperty<decimal?>(VOWBIdPropertyName);
            }
        }

        public string VOWBName
        {
            get
            {
                return GetProperty<string>(VOWBNamePropertyName);
            }
        }

        public string ArtCode_R
        {
            get
            {
                return GetProperty<string>(ArtCode_RPropertyName);
            }
        }

        public string VArtName
        {
            get
            {
                return GetProperty<string>(VArtNamePropertyName);
            }
        }

        public string VArtDesc
        {
            get
            {
                return GetProperty<string>(VArtDescPropertyName);
            }
        }

        public string SKUName
        {
            get
            {
                return GetProperty<string>(SKUNamePropertyName);
            }
        }

        public decimal TRANSPORTTASKID_R
        {
            get { return GetProperty<decimal>(TRANSPORTTASKID_RPropertyName); }
            set { SetProperty(TRANSPORTTASKID_RPropertyName, value); }
        }

        public decimal OWBPosID
        {
            get { return GetProperty<decimal>(OWBPosIDPropertyName); }
            set { SetProperty(OWBPosIDPropertyName, value); }
        }

        public decimal? MandantID
        {
            get
            {
                return GetProperty<decimal?>(MandantIDPropertyName);
            }
        }

        public decimal? PRODUCTOWNER
        {
            get { return GetProperty<decimal?>(PRODUCTOWNERPropertyName); }
            set { SetProperty(PRODUCTOWNERPropertyName, value); }
        }

        public string PRODUCTKITARTNAME
        {
            get { return GetProperty<string>(PRODUCTKITARTNAMEPropertyName); }
            set { SetProperty(PRODUCTKITARTNAMEPropertyName, value); }
        }

        public DateTime? PRODUCTDATE
        {
            get { return GetProperty<DateTime?>(PRODUCTDATEPropertyName); }
            set { SetProperty(PRODUCTDATEPropertyName, value); }
        }

        public string PRODUCTLOT
        {
            get { return GetProperty<string>(PRODUCTLOTPropertyName); }
            set { SetProperty(PRODUCTLOTPropertyName, value); }
        }

        #endregion .  Properties  .
    }
}