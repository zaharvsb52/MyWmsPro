namespace wmsMLC.Business.Objects
{
    public class Product2Blocking : WMSBusinessObject
    {
        #region .  Constants  .
        public const string Product2BlockingIdPropertyName = "PRODUCT2BLOCKINGID";
        public const string ProductIdPropertyName = "PRODUCT2BLOCKINGPRODUCTID";
        public const string BlockingCodePropertyName = "PRODUCT2BLOCKINGBLOCKINGCODE";
        public const string Product2BlockingDescPropertyName = "PRODUCT2BLOCKINGDESC";
        #endregion

        #region .  Properties  .
        public decimal? Product2BlockingId
        {
            get { return GetProperty<decimal?>(Product2BlockingIdPropertyName); }
            set { SetProperty(Product2BlockingIdPropertyName, value); }
        }

        public string Product2BlockingProductCode
        {
            get { return GetProperty<string>(ProductIdPropertyName); }
            set { SetProperty(ProductIdPropertyName, value); }
        }

        public string Product2BlockingBlockingCode
        {
            get { return GetProperty<string>(BlockingCodePropertyName); }
            set { SetProperty(BlockingCodePropertyName, value); }
        }

        public string Product2BlockingDesc
        {
            get { return GetProperty<string>(Product2BlockingDescPropertyName); }
            set { SetProperty(Product2BlockingDescPropertyName, value); }
        }
        #endregion
    }
}