using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectView))]
    public class TEViewModel : ObjectViewModelBase<TE>
    {
        protected override void OnSourceChanged()
        {
            base.OnSourceChanged();
            if (Source == null)
                return;

            if (!Source.IsNew)
                return;

            var editable = Source as IEditable;
            if (editable.IsInRejectChanges)
                return;

            var mgr = (ITEManager)GetManager();
            mgr.FillCreationPlace(Source);
        }
        
        protected override void SourceObjectPropertyChanged(object sender,
                                                            System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.SourceObjectPropertyChanged(sender, e);

            if (Source == null || !Source.IsNew)
                return;

            var editable = Source as IEditable;
            if (editable.IsInRejectChanges)
                return;

            if (!e.PropertyName.EqIgnoreCase(TE.CreatePlacePropertyName) &&
                !e.PropertyName.EqIgnoreCase(TE.TETypeCodePropertyName))
                return;

            if (e.PropertyName.EqIgnoreCase(TE.TETypeCodePropertyName))
            {
                var mgr = (ITEManager)GetManager();
                mgr.FillDimensionCharacteristics(Source);
                return;
            }

            if (Source.CurrentPlace == null)
                Source.CurrentPlace = Source.CreatePlace;
        }
    }
}