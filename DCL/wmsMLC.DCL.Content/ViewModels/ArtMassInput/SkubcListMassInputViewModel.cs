using System.Collections.Generic;
using wmsMLC.Business.Objects;

namespace wmsMLC.DCL.Content.ViewModels.ArtMassInput
{
    public class SKUBCListMassInputViewModel : ListMassInputViewModel<SKUBC>
    {
        private static readonly List<string> _excludedSKUBCFields = new List<string>()
        {
            new SKUBC().GetPrimaryKeyPropertyName(),
            SKUBC.BarcodeEntityPropertyName,
            SKUBC.BarcodeKeyPropertyName,
            SKUBC.UserInsPropertyName,
            SKUBC.DateInsPropertyName,
            SKUBC.UserUpdPropertyName,
            SKUBC.DateUpdPropertyName,
            SKUBC.TransactPropertyName
        };

        public SKUBCListMassInputViewModel(bool canHandleHotKeys)
            : base(canHandleHotKeys)
        {
        }

        protected override List<string> ExcludedFields
        {
            get { return _excludedSKUBCFields; }
        }
    }
}