namespace wmsMLC.Business.Objects
{
    public class ArtPrice : WMSBusinessObject
    {
        #region .  Constants  .
        public const string ArtPriceIDPropertyName = "ARTPRICEID";
        public const string ArtPriceValuePropertyName = "ARTPRICEVALUE";
        public const string ArtPriceSKUIDPropertyName = "SKUID_R";
        public const string ArtPriceMandantIDPropertyName = "MANDANTID";
        #endregion

        #region .  Properties  .
        public decimal? ArtPriceID
        {
            get { return GetProperty<decimal?>(ArtPriceIDPropertyName); }
            set { SetProperty(ArtPriceIDPropertyName, value); }
        }
        public double? ArtPriceValue
        {
            get { return GetProperty<double?>(ArtPriceValuePropertyName); }
            set { SetProperty(ArtPriceValuePropertyName, value); }
        }
        public decimal ArtPriceSKUID
        {
            get { return GetProperty<decimal>(ArtPriceSKUIDPropertyName); }
            set { SetProperty(ArtPriceSKUIDPropertyName, value); }
        }
        #endregion
    }
}