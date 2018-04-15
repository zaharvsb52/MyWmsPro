using System.Windows;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Resources;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectView))]
    public class ExpiryDateViewModel : ObjectViewModelBase<ExpiryDate>
    {
        protected override void OnSave()
        {
            if (!CanSave() || Source == null)
                return;

            if (!CheckUsingOption())
                return;

            base.OnSave();
        }

        protected override void OnSaveAndClose()
        {
            if (!CanSave() || Source == null)
                return;

            if (!CheckUsingOption())
                return;

            base.OnSaveAndClose();
        }

        private bool CheckUsingOption()
        {
            if (string.IsNullOrEmpty(Source.ExpiryDateUsingOption) || !string.Equals(Source.ExpiryDateUsingOption, "LOCAL"))
                return  true;

            if (Source.ExpiryDateValue != null && Source.ExpiryDateValueType != null)
                return true;

            var message = string.Empty;

            if (Source.ExpiryDateValue == null)
                message = string.Format(StringResources.ErrorSaveShouldNotBeEmpty, "Значение");

            if (string.IsNullOrWhiteSpace(Source.ExpiryDateValueType))
                message = string.IsNullOrEmpty(message)
                    ? string.Format(StringResources.ErrorSaveShouldNotBeEmpty, "Тип значения")
                    : string.Format("{0}\n{1}", message, string.Format(StringResources.ErrorSaveShouldNotBeEmpty, "Тип значения"));

            GetViewService().ShowDialog(StringResources.Error, message, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
            return false;
        }
    }
}