using wmsMLC.Business.Objects;
using wmsMLC.General.BL;

namespace wmsMLC.DCL.Content.ViewModels.ArtMassInput
{
    [SysObjectName("SKU")]
    public class SKUItem : SKU
    {
        public Sku2TteListMassInputViewModel SKU2TTEList { get; set; }
        public ArtPriceListMassInputViewModel ArtPriceList { get; set; }
        public SKUBCListMassInputViewModel SKUBCList { get; set; }

        public SKUItem()
        {
            SKU2TTEList = new Sku2TteListMassInputViewModel(false);
            ArtPriceList = new ArtPriceListMassInputViewModel(false);
            SKUBCList = new SKUBCListMassInputViewModel(false);
        }

        public decimal Value
        {
            get { return SKUID; }
            set { SKUID = value; }
        }

        public string Display
        {
            get { return SKUName; }
            set { SKUName = value; }
        }
    }
}