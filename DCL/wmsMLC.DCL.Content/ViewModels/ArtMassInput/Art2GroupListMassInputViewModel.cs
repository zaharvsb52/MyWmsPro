using System.Collections.Generic;
using wmsMLC.Business.Objects;

namespace wmsMLC.DCL.Content.ViewModels.ArtMassInput
{
    public class Art2GroupListMassInputViewModel : ListMassInputViewModel<Art2Group>
    {
        private static readonly List<string> _excludedArt2GroupFields = new List<string>()
        {
            Art2Group.Art2GroupIDPropertyName,
            Art2Group.Art2GroupArtCodePropertyName,
            Art2Group.UserInsPropertyName,
            Art2Group.DateInsPropertyName,
            Art2Group.UserUpdPropertyName,
            Art2Group.DateUpdPropertyName,
            Art2Group.TransactPropertyName,
            Art2Group.Art2GroupMandantPropertyName

        };

        public Art2GroupListMassInputViewModel(bool canHandleHotKeys)
            : base(canHandleHotKeys)
        {
        }

        protected override List<string> ExcludedFields
        {
            get { return _excludedArt2GroupFields; }
        }

        protected override void SetDefaultValues(Art2Group item)
        {
            base.SetDefaultValues(item);
            item.Art2GroupArtCode = "dummy";
        }
    }
}