namespace wmsMLC.Business.Objects
{
    public class ProductBlocking : WMSBusinessObject
    {
        #region .  Constants  .
        public const string BlockingCodePropertyName = "BlockingCode";
        public const string BlockingNamePropertyName = "BlockingName";

        public const string BlockingForProductPropertyName = "BlockingForProduct";
        public const string BlockingForTEPropertyName = "BlockingForTE";
        public const string BlockingForPlacePropertyName = "BlockingForPlace";
        #endregion

        #region .  Properties  .
        public string BlockingCode
        {
            get { return GetProperty<string>(BlockingCodePropertyName); }
            set { SetProperty(BlockingCodePropertyName, value); }
        }
        public string BlockingName
        {
            get { return GetProperty<string>(BlockingNamePropertyName); }
            set { SetProperty(BlockingNamePropertyName, value); }
        }
        #endregion
    }
}
