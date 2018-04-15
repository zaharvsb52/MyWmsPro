using System;
using BLToolkit.Reflection;
using wmsMLC.General;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.DCL.General.Helpers
{
    public abstract class ConnectionManager
    {
        #region .  Singleton realization  .
        private static readonly Lazy<ConnectionManager> _instance = new Lazy<ConnectionManager>(TypeAccessor.CreateInstance<ConnectionManager>);
        public static ConnectionManager Instance
        {
            get { return _instance.Value; }
        }
        #endregion

        #region . Methods .

        public virtual bool AllowRequest()
        {
            if (WMSEnvironment.Instance.IsConnected != false)
                return true;
            var vs = IoC.Instance.Resolve<IViewService>();
            if (vs.ShowDialog("Отсутствует связь с сервисом", string.Format("Связь с сервисом была утеряна {0}.\r\nСистема пытается восстановить связь в автоматическом режиме.\r\nВсе равно попробовать выполнить запрос?", WMSEnvironment.Instance.ConnectionFaultTime), System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question, System.Windows.MessageBoxResult.No) == System.Windows.MessageBoxResult.Yes)
                return true;
            return false;
        }

        #endregion
    }
}
