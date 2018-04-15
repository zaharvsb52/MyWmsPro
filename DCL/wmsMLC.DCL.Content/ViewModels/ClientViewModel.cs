using System.Windows;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectView))]    
    public class ClientViewModel : ObjectViewModelBase<Client>
    {
        private string _oldTruckCode;
        private bool _hasOldValue;

        protected override void OnSourceChanged()
        {
            base.OnSourceChanged();

            if (Source == null || _hasOldValue)
                return;

            _oldTruckCode = Source.TruckCode_R;
            _hasOldValue = true;
        }

        protected override bool Save()
        {
            var result = base.Save();
            if (!result)
                return false;

            try
            {
                WaitStart();

                var truckCode = Source.TruckCode_R;
                if (_oldTruckCode != truckCode)
                {
                    _oldTruckCode = truckCode;
                    var clientCode = Source.GetKey<string>();
                    using (var mng = (IClientSessionManager)IoC.Instance.Resolve<IBaseManager<ClientSession>>())
                    {
                        result = mng.CloseRclSession(clientCode, truckCode);
                    }

                    if (result)
                    {
                        GetViewService().ShowDialog(StringResources.Information,
                            string.Format(StringResources.ClientChangeTruckMessageFormat, clientCode),
                            MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                    }
                }
                return true;
            }
            finally
            {
                WaitStop();
            }
        }
    }
}
