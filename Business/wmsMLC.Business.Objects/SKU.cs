namespace wmsMLC.Business.Objects
{
    public class SKU : WMSBusinessObject
    {
        #region .  Constants  .
        public const string ArtCodePropertyName = "ARTCODE_R";
        public const string MeasureCodePropertyName = "MEASURECODE_R";
        public const string SKUWeightPropertyName = "SKUWEIGHT";
        public const string SKUCountPropertyName = "SKUCOUNT";
        public const string SKUNamePropertyName = "SKUNAME";
        public const string VARTDESCPropertyName = "VARTDESC";
        public const string VMEASURENAMEPropertyName = "VMEASURENAME";
        public const string SKUClientPropertyName = "SKUCLIENT";
        public const string SKUPrimaryPropertyName = "SKUPRIMARY";
        public const string SKUParentPropertyName = "SKUPARENT";
        public const string SKUVolumePropertyName = "SKUVOLUME";
        public const string SKUIDPropertyName = "SKUID";
        public const string SKUDefaultPropertyName = "SKUDEFAULT";
        
        public const string SKUMandantIDPropertyName = "MANDANTID";

        public const string SKUClientLengthPropertyName = "SKULENGTHCL";
        public const string SKUClientWidthPropertyName = "SKUWIDTHCL";
        public const string SKUClientHeightPropertyName = "SKUHEIGHTCL";
        public const string SKUClientWeightPropertyName = "SKUWEIGHTCL";
        public const string SKUClientVolumePropertyName = "SKUVOLUMECL";
        public const string SKUTETypePropertyName = "TETYPE2SKU";
        public const string SKUArtPricePropertyName = "ARTPRICEL";
        public const string SKUBarCodePropertyName = "BARCODEL";
        public const string SKUCPVPropertyName = "SKUCPV";
        public const string SKULENGTHPropertyName = "SKULENGTH";
        public const string SKUWIDTHPropertyName = "SKUWIDTH";
        public const string SKUHEIGHTPropertyName = "SKUHEIGHT";
        public const string SKUINDIVISIBLEPropertyName = "SKUINDIVISIBLE";
        public const string SKUCRITBATCHPropertyName = "SKUCRITBATCH";
        public const string SKUCRITPLPropertyName = "SKUCRITPL";
        public const string SKUCRITMSCPropertyName = "SKUCRITMSC";
        public const string SKURESERVTYPEPropertyName = "SKURESERVTYPE";
        public const string SKUDESCPropertyName = "SKUDESC";
        #endregion .  Constants  .

        #region .  Properties  .
        public string ArtCode
        {
            get { return GetProperty<string>(ArtCodePropertyName); }
            set { SetProperty(ArtCodePropertyName, value); }
        }

        public string MeasureCode
        {
            get { return GetProperty<string>(MeasureCodePropertyName); }
            set { SetProperty(MeasureCodePropertyName, value); }
        }

        public decimal SKUWeight
        {
            get { return GetProperty<decimal>(SKUWeightPropertyName); }
            set { SetProperty(SKUWeightPropertyName, value); }
        }

        public double SKUCount
        {
            get { return GetProperty<double>(SKUCountPropertyName); }
            set { SetProperty(SKUCountPropertyName, value); }
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
        public string SKUName
        {
            get { return GetProperty<string>(SKUNamePropertyName); }
            set { SetProperty(SKUNamePropertyName, value); }
        }
        public bool SKUClient
        {
            get { return GetProperty<bool>(SKUClientPropertyName); }
            set { SetProperty(SKUClientPropertyName, value); }
        }
        public bool SKUPrimary
        {
            get { return GetProperty<bool>(SKUPrimaryPropertyName); }
            set { SetProperty(SKUPrimaryPropertyName, value); }
        }
        public decimal? SKUParent
        {
            get { return GetProperty<decimal?>(SKUParentPropertyName); }
            set { SetProperty(SKUParentPropertyName, value); }
        }
        public decimal SKUVolume
        {
            get { return GetProperty<decimal>(SKUVolumePropertyName); }
            set { SetProperty(SKUVolumePropertyName, value); }
        }
        public decimal SKUID
        {
            get { return GetProperty<decimal>(SKUIDPropertyName); }
            set { SetProperty(SKUIDPropertyName, value); }
        }
        public bool SKUDefault
        {
            get { return GetProperty<bool>(SKUDefaultPropertyName); }
            set { SetProperty(SKUDefaultPropertyName, value); }
        }

        public WMSBusinessCollection<SKU2TTE> SKU2TTEL
        {
            get { return GetProperty<WMSBusinessCollection<SKU2TTE>>(SKUTETypePropertyName); }
            set { SetProperty(SKUTETypePropertyName, value); }
        }

        public WMSBusinessCollection<ArtPrice> ArtPriceL
        {
            get { return GetProperty<WMSBusinessCollection<ArtPrice>>(SKUArtPricePropertyName); }
            set { SetProperty(SKUArtPricePropertyName, value); }
        }

        public WMSBusinessCollection<SKUBC> SKUBCL
        {
            get { return GetProperty<WMSBusinessCollection<SKUBC>>(SKUBarCodePropertyName); }
            set { SetProperty(SKUBarCodePropertyName, value); }
        }
        #endregion .  Properties  .
    }
}