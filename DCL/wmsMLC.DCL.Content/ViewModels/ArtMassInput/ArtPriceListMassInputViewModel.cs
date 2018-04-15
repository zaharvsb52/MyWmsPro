using System.Collections.Generic;
using wmsMLC.Business.Objects;

namespace wmsMLC.DCL.Content.ViewModels.ArtMassInput
{
    public class ArtPriceListMassInputViewModel : ListMassInputViewModel<ArtPrice>
    {
        private static readonly List<string> _excludedArtPriceFields = new List<string>()
        {
            ArtPrice.ArtPriceIDPropertyName,
            ArtPrice.ArtPriceSKUIDPropertyName,
            ArtPrice.UserInsPropertyName,
            ArtPrice.DateInsPropertyName,
            ArtPrice.UserUpdPropertyName,
            ArtPrice.DateUpdPropertyName,
            ArtPrice.TransactPropertyName,
            ArtPrice.ArtPriceMandantIDPropertyName
        };

        protected override List<string> ExcludedFields
        {
            get { return _excludedArtPriceFields; }
        }

        public ArtPriceListMassInputViewModel(bool canHandleHotKeys)
            : base(canHandleHotKeys)
        {
        }

        protected override void SetDefaultValues(ArtPrice item)
        {
            base.SetDefaultValues(item);
            item.ArtPriceSKUID = -1;
        }
    }
}