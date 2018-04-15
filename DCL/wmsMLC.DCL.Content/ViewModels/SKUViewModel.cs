using System.Windows;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof (ObjectView))]
    public class SKUViewModel : ObjectViewModelBase<SKU>
    {
        private decimal? _oldParent = null;

        protected override void SourceObjectPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.SourceObjectPropertyChanged(sender, e);

            if (Source == null)
                return;

            var editable = Source as IEditable;
            if (editable.IsInRejectChanges)
                return;

            if (!e.PropertyName.EqIgnoreCase(SKU.SKUPrimaryPropertyName))
                return;

            if (Source.SKUPrimary)
            {
                _oldParent = Source.SKUParent;
                Source.SKUParent = null;
                return;
            }

            if (Source.SKUParent != null) 
                return;

            Source.SKUParent = _oldParent;
            _oldParent = null;
        }


        protected override void OnSave()
        {
            if (!CanSave() || Source == null)
                return;

            if (!CheckParent())
                return;

            base.OnSave();
        }

        protected override void OnSaveAndClose()
        {
            if (!CanSave() || Source == null)
                return;

            if(!CheckParent())
                return;
            
            base.OnSaveAndClose();
        }

        private bool CheckParent()
        {
            if (Source.SKUPrimary)
            {
                if (Source.SKUParent == null)
                    return true;
                GetViewService().ShowDialog(StringResources.Error, StringResources.ErrorSaveSKUParentShouldBeEmpty, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                return false;
            }

            if (Source.SKUParent != null)
                return true;
            GetViewService().ShowDialog(StringResources.Error, StringResources.ErrorSaveSKUParentShouldNotBeEmpty, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
            return false;
        }
    }
}