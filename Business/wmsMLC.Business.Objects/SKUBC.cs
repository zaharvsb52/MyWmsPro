using wmsMLC.General;

namespace wmsMLC.Business.Objects
{
    [SourceName("BARCODE")]
    public class SKUBC : Barcode
    {
        #region .  Constants  .
        public new const string BarcodeEntityPropertyName = "BARCODE2ENTITY_SKUBC";
        public new const string BarcodeKeyPropertyName = "BARCODEKEY_SKUBC";
        public new const string BarcodeValuePropertyName = "BARCODEVALUE_SKUBC";
        #endregion .  Constants  .

        #region .  Properties  .
        public new string BarcodeKey
        {
            get { return GetProperty<string>(BarcodeKeyPropertyName); }
            set { SetProperty(BarcodeKeyPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}