using System.Linq;
using System.Windows;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Resources;
using wmsMLC.General.PL;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof (ObjectView))]
    public class PartnerViewModel : ObjectViewModelBase<Partner>
    {
        protected override void OnSave()
        {
            if (!CanSave() || Source == null)
                return;

            if (!CheckCommercTime())
                return;

            base.OnSave();
        }

        protected override void OnSaveAndClose()
        {
            if (!CanSave() || Source == null)
                return;

            if (!CheckCommercTime())
                return;

            base.OnSaveAndClose();
        }

        private bool CheckCommercTime()
        {
            if ((Source.PartnerCommercTime == null && string.IsNullOrEmpty(Source.PartnerCommercTimeMeasure)) ||
                (Source.PartnerCommercTime != null && !string.IsNullOrEmpty(Source.PartnerCommercTimeMeasure)))
                return true;

            var fields = GetDataFields(SettingDisplay.Detail);
            var field = Source.PartnerCommercTime == null ? fields.FirstOrDefault(x => x.SourceName == Partner.PARTNERCOMMERCTIMEPropertyName) : fields.FirstOrDefault(x => x.SourceName == Partner.PARTNERCOMMERCTIMEMEASUREPropertyName);

            if (field == null)
                return true;

            GetViewService().ShowDialog(StringResources.Error, string.Format(StringResources.ErrorSaveShouldNotBeEmpty, field.Caption), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
            return false;
        }
    }
}