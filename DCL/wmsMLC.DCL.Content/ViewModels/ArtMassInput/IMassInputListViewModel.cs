using System;

namespace wmsMLC.DCL.Content.ViewModels.ArtMassInput
{
    public interface IMassInputListViewModel
    {
        event EventHandler<AddItemEventArgs> ItemAdded;
        void SetSelectedItem(object item);
    }
}