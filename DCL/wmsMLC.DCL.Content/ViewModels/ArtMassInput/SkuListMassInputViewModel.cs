using System.Collections.Generic;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.DCL.Content.ViewModels.ArtMassInput
{
    public class SkuListMassInputViewModel : ListMassInputViewModel<SKUItem>
    {
        private int id = 0;

        private static readonly List<string> _excludedSKUFields = new List<string>()
        {
            SKU.SKUIDPropertyName,
            SKU.SKUMandantIDPropertyName,
            SKU.ArtCodePropertyName,
            SKU.SKUVolumePropertyName,
            SKU.VARTDESCPropertyName,
            SKU.UserInsPropertyName,
            SKU.DateInsPropertyName,
            SKU.UserUpdPropertyName,
            SKU.DateUpdPropertyName,
            SKU.TransactPropertyName,
            SKU.SKUClientLengthPropertyName,
            SKU.SKUClientWidthPropertyName, 
            SKU.SKUClientHeightPropertyName,
            SKU.SKUClientWeightPropertyName,
            SKU.SKUClientVolumePropertyName,
            SKU.SKUTETypePropertyName,
            SKU.SKUArtPricePropertyName,
            SKU.SKUBarCodePropertyName,
            SKU.SKUCPVPropertyName
        };

        protected override List<string> ExcludedFields
        {
            get { return _excludedSKUFields; }
        }

        private static readonly List<string> _fieldsOrder = new List<string>()
        {
            SKU.SKUNamePropertyName,
            SKU.MeasureCodePropertyName,
            SKU.SKUCountPropertyName,
            SKU.SKUWeightPropertyName,
            SKU.SKULENGTHPropertyName,
            SKU.SKUWIDTHPropertyName,
            SKU.SKUHEIGHTPropertyName,
            SKU.SKUParentPropertyName,
            SKU.SKUINDIVISIBLEPropertyName,
            SKU.SKUClientPropertyName,
            SKU.SKUPrimaryPropertyName,
            SKU.SKUDefaultPropertyName,
            SKU.SKUCRITBATCHPropertyName,
            SKU.SKUCRITPLPropertyName,
            SKU.SKUCRITMSCPropertyName,
            SKU.SKURESERVTYPEPropertyName,
            SKU.SKUDESCPropertyName
        };

        protected override List<string> FieldsOrder
        {
            get { return _fieldsOrder; }
        }

        public SkuListMassInputViewModel(bool canHandleHotKeys)
            : base(canHandleHotKeys)
        {
        }

        protected override void SetDefaultValues(SKUItem item)
        {
            base.SetDefaultValues(item);
            item.ArtCode = "dummy";
        }

        protected override object GetNextId()
        {
            id--;
            return id;
        }

        protected override void New()
        {
            using (var mgr = IoC.Instance.Resolve<IBaseManager<SKU>>())
            {
                var sku = mgr.New();
                var skuItem = new SKUItem();
                Mapper.Map(sku, skuItem);
                SetDefaultValues(skuItem);
                SetPrimaryKeyValue(skuItem);
                Validate(skuItem);
                Items.Add(skuItem);
                OnItemAdded(skuItem);
            }
        }
    }
}