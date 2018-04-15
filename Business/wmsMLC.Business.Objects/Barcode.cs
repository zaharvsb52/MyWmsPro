namespace wmsMLC.Business.Objects
{
    public class Barcode : WMSBusinessObject
    {
        #region .  Constants  .
        public const string BarcodeIDPropertyName = "BARCODEID";
        public const string BarcodeValuePropertyName = "BARCODEVALUE";
        public const string BarcodeKeyPropertyName = "BARCODEKEY";
        public const string BarcodeEntityPropertyName = "BARCODE2ENTITY";
        #endregion
        #region .  Properties  .
        public decimal BarcodeID
        {
            get { return GetProperty<decimal>(BarcodeIDPropertyName); }
            set { SetProperty(BarcodeIDPropertyName, value); }
        }
        public string BarcodeValue
        {
            get { return GetProperty<string>(BarcodeValuePropertyName); }
            set { SetProperty(BarcodeValuePropertyName, value); }
        }
        public string BarcodeKey
        {
            get { return GetProperty<string>(BarcodeKeyPropertyName); }
            set { SetProperty(BarcodeKeyPropertyName, value); }
        }
        public string BarcodeEntity
        {
            get { return GetProperty<string>(BarcodeEntityPropertyName); }
            set { SetProperty(BarcodeEntityPropertyName, value); }
        }
        #endregion
    }
}