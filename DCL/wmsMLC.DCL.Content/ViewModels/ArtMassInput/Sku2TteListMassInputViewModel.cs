using System.Collections.Generic;
using wmsMLC.Business.Objects;

namespace wmsMLC.DCL.Content.ViewModels.ArtMassInput
{
    public class Sku2TteListMassInputViewModel : ListMassInputViewModel<SKU2TTE>
    {
        private static readonly List<string> _excludedSKU2TTEFields = new List<string>()
        {
            SKU2TTE.SKU2TTEIDPropertyName,
            SKU2TTE.SKU2TTESKUIDPropertyName,
            SKU2TTE.UserInsPropertyName,
            SKU2TTE.DateInsPropertyName,
            SKU2TTE.UserUpdPropertyName,
            SKU2TTE.DateUpdPropertyName,
            SKU2TTE.TransactPropertyName,
            SKU2TTE.SKU2TTEMandantIDPropertyName
        };

        protected override List<string> ExcludedFields
        {
            get { return _excludedSKU2TTEFields; }
        }

        private static readonly List<string> _fieldsOrder = new List<string>()
        {
            SKU2TTE.TETypeCodePropertyName,
            SKU2TTE.SKU2TTEQuantityPropertyName,
            SKU2TTE.SKU2TTEQuantityMaxPropertyName,
            SKU2TTE.SKU2TTELENGTHPropertyName,
            SKU2TTE.SKU2TTEWIDTHPropertyName,
            SKU2TTE.SKU2TTEHeightPropertyName,
            SKU2TTE.SKU2TTEMaxWeightPropertyName,
            SKU2TTE.SKU2TTEDefaultPropertyName,
            SKU2TTE.SKU2TTECRITMSCPropertyName,
            SKU2TTE.SKU2TTESELMMPropertyName
        };

        protected override List<string> FieldsOrder
        {
            get { return _fieldsOrder; }
        }

        public Sku2TteListMassInputViewModel(bool canHandleHotKeys)
            : base(canHandleHotKeys)
        {
        }

        protected override void SetDefaultValues(SKU2TTE item)
        {
            base.SetDefaultValues(item);
            item.SKU2TTESKUID = -1;
        }
    }
}