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
    public class CargoOWBViewModel : ObjectViewModelBase<CargoOWB>
    {
        private bool _isNeededClearAddress;

        protected override void OnSourceChanged()
        {
            _isNeededClearAddress = false;
            base.OnSourceChanged();

            if (Source != null && Source.CargoOwbUnloadAddress == null)
            {
                Source.CargoOwbUnloadAddress = new AddressBook();
                Source.CargoOwbUnloadAddress.AcceptChanges();
                _isNeededClearAddress = true;
            }
        }

        protected override void SourceObjectPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.SourceObjectPropertyChanged(sender, e);

            if (Source == null)
                return;

            if (!e.PropertyName.EqIgnoreCase(CargoOWB.INTERNALTRAFFICID_RPropertyName))
                return;

            var editable = Source as IEditable;
            if (editable.IsInRejectChanges)
                return;

            var mgr = (ICargoManager) GetManager();
            mgr.FillData(Source);
        }

        protected override bool Save()
        {
            var result = false;
            var cargoOwbUnloadAddress = (AddressBook)Source.CargoOwbUnloadAddress.Clone();

            try
            {
                if (_isNeededClearAddress && Source != null && Source.CargoOwbUnloadAddress != null &&
                    !Source.CargoOwbUnloadAddress.IsDirty)
                {
                    Source.CargoOwbUnloadAddress = null;
                }

                result = base.Save();
                return result;
            }
            finally
            {
                if (result)
                    _isNeededClearAddress = false;
                else
                    Source.CargoOwbUnloadAddress = cargoOwbUnloadAddress;
            }
        }
    }
}