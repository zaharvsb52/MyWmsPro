using System.ComponentModel;
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
    public class CargoIWBViewModel : ObjectViewModelBase<CargoIWB>
    {
        private bool _isNeededClearAddress;

        protected override void OnSourceChanged()
        {
            _isNeededClearAddress = false;
            base.OnSourceChanged();

            if (Source != null && Source.CargoIwbLoadAddress == null)
            {
                Source.CargoIwbLoadAddress = new AddressBook();
                Source.CargoIwbLoadAddress.AcceptChanges();
                _isNeededClearAddress = true;
            }
        }

        protected override void SourceObjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.SourceObjectPropertyChanged(sender, e);

            if (Source == null)
                return;

            if (!e.PropertyName.EqIgnoreCase(CargoIWB.InternalTrafficIDPropertyName))
                return;

            var editable = Source as IEditable;
            if (editable.IsInRejectChanges)
                return;

            var mgr = (ICargoManager)GetManager();
            mgr.FillData(Source);
        }

        protected override bool Save()
        {
            var result = false;
            var cargoIwbLoadAddress = (AddressBook) Source.CargoIwbLoadAddress.Clone();

            try
            {
                if (_isNeededClearAddress && Source != null && Source.CargoIwbLoadAddress != null &&
                    !Source.CargoIwbLoadAddress.IsDirty)
                {
                    Source.CargoIwbLoadAddress = null;
                }

                result = base.Save();
                return result;
            }
            finally
            {
                if (result)
                    _isNeededClearAddress = false;
                else
                    Source.CargoIwbLoadAddress = cargoIwbLoadAddress;
            }
        }
    }
}