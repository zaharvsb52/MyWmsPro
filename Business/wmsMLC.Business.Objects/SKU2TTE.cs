namespace wmsMLC.Business.Objects
{
    public class SKU2TTE : WMSBusinessObject
    {
        #region .  Constants  .
        public const string SKU2TTEHeightPropertyName = "SKU2TTEHEIGHT";
        public const string SKU2TTELengthPropertyName = "SKU2TTELENGTH";
        public const string SKU2TTEWidthPropertyName = "SKU2TTEWIDTH";
        public const string SKU2TTEDefaultPropertyName = "SKU2TTEDEFAULT";
        public const string TETypeCodePropertyName = "SKU2TTETETYPECODE";
        public const string SKU2TTEQuantityPropertyName = "SKU2TTEQUANTITY";
        public const string SKU2TTEQuantityMaxPropertyName = "SKU2TTEQUANTITYMAX";
        public const string SKU2TTEIDPropertyName = "SKU2TTEID";
        public const string SKU2TTESKUIDPropertyName = "SKU2TTESKUID";
        public const string SKU2TTEMaxWeightPropertyName = "SKU2TTEMAXWEIGHT";
        public const string SKU2TTEMandantIDPropertyName = "MANDANTID";
        public const string SKU2TTELENGTHPropertyName = "SKU2TTELENGTH";
        public const string SKU2TTEWIDTHPropertyName = "SKU2TTEWIDTH";
        public const string SKU2TTECRITMSCPropertyName = "SKU2TTECRITMSC";
        public const string SKU2TTESELMMPropertyName = "SKU2TTESELMM";
        public const string VSKUNAMEPropertyName = "VSKUNAME";
        #endregion .  Constants  .

        #region .  Properties  .
        public decimal SKU2TTEHeight
        {
            get { return GetProperty<decimal>(SKU2TTEHeightPropertyName); }
            set { SetProperty(SKU2TTEHeightPropertyName, value); }
        }
        public decimal SKU2TTELength
        {
            get { return GetProperty<decimal>(SKU2TTELengthPropertyName); }
            set { SetProperty(SKU2TTELengthPropertyName, value); }
        }
        public decimal SKU2TTEWidth
        {
            get { return GetProperty<decimal>(SKU2TTEWidthPropertyName); }
            set { SetProperty(SKU2TTEWidthPropertyName, value); }
        }

        public bool SKU2TTEDefault
        {
            get { return GetProperty<bool>(SKU2TTEDefaultPropertyName); }
            set { SetProperty(SKU2TTEDefaultPropertyName, value); }
        }

        public string TETypeCode
        {
            get { return GetProperty<string>(TETypeCodePropertyName); }
            set { SetProperty(TETypeCodePropertyName, value); }
        }

        public decimal SKU2TTEQuantity
        {
            get { return GetProperty<decimal>(SKU2TTEQuantityPropertyName); }
            set { SetProperty(SKU2TTEQuantityPropertyName, value); }
        }

        public decimal SKU2TTEQuantityMax
        {
            get { return GetProperty<decimal>(SKU2TTEQuantityMaxPropertyName); }
            set { SetProperty(SKU2TTEQuantityMaxPropertyName, value); }
        }

        public decimal SKU2TTEID
        {
            get { return GetProperty<decimal>(SKU2TTEIDPropertyName); }
            set { SetProperty(SKU2TTEIDPropertyName, value); }
        }
        public decimal SKU2TTESKUID
        {
            get { return GetProperty<decimal>(SKU2TTESKUIDPropertyName); }
            set { SetProperty(SKU2TTESKUIDPropertyName, value); }
        }

        public decimal SKU2TTEMaxWeight
        {
            get { return GetProperty<decimal>(SKU2TTEMaxWeightPropertyName); }
            set { SetProperty(SKU2TTEMaxWeightPropertyName, value); }
        }
        public string VSKUNAME
        {
            get { return GetProperty<string>(VSKUNAMEPropertyName); }
            set { SetProperty(VSKUNAMEPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}