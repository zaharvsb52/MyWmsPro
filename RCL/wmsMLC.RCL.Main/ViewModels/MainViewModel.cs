using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using wmsMLC.Business;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.General.BL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Components.ViewModels;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.ViewModels;
using wmsMLC.General;
using wmsMLC.RCL.Main.Helpers;

namespace wmsMLC.RCL.Main.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region .  Fields & Consts  .
        public const string UserInfoPropertyName = "UserInfo";
        private const string SysDbInfoNone = "?";
        //private const int DefaultTimeout = 30000;

#if DEBUG
#else
        private const int DefaultPingTime = 5000;
        private const int GoodPing = 5; // хороший пинг
        private readonly Timer _pinger;
#endif
        private static volatile bool _inLogOff;
        private const int StatusBarItemMaxLength = 15;

        private string _state;
        private int _pingStateIndex;
        private readonly IAuthenticationProvider _authenticationProvider;
        private string _systemMessage;
        private string _systemMessageSubject;
        private string _systemMessageTime;
        private string _systemMessageCount;
        private int _sysMsgState;
        private List<string> _criticalErrorMessages;
        #endregion.  Fields & Consts  .

        public MainViewModel()
        {
            _criticalErrorMessages = new List<string>();
            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
            {
                _authenticationProvider = IoC.Instance.Resolve<IAuthenticationProvider>();
                _authenticationProvider.AuthenticatedUserChanged += (sender, args) =>
                    {
                        OnPropertyChanged(UserInfoPropertyName);
                        UpdateSysEnvironmentInfo(GetSysDbInfo());
                    };
                OnPropertyChanged(UserInfoPropertyName);

                // запускаем пинг
                // В дебаге пинг не нужен, а проблем из-за отладки с точками останова - огромное кол-во
                // INFO: ОТКЛЮЧАЕМ ПИНГ
//#if !DEBUG
//                _pinger = new Timer(CheckStatus, null, 0, DefaultPingTime);
//#endif
            }
            PingStateIndex = 0;
            _sysMsgState = 0;

            LoginCommand = new DelegateCustomCommand(Login, CanReAuthenticate);
            LogoffCommand = new DelegateCustomCommand(Logoff, () => true);
            ClearCacheCommand = new DelegateCustomCommand(ClearCache, () => true);
            MainMenuShowCommand = new DelegateCustomCommand(ShowMainMenu, () => true);
            SysDbInfoCommand = new DelegateCustomCommand(OnSysDbInfo, () => true);

            SystemMessageSubject = Resources.StringResources.StateRun;
            SystemMessage = string.Format(Resources.StringResources.EnjoyYourWork, Environment.NewLine);
        }

        #region .  Properties  .
        public string UserInfo
        {
            get
            {
                var orig = _authenticationProvider == null || _authenticationProvider.AuthenticatedUser == null
                                  ? Resources.StringResources.None
                                  : _authenticationProvider.AuthenticatedUser.GetSignature();
                var message = string.Format("[{0}]", orig);
                if (message.Length > StatusBarItemMaxLength)
                    orig = orig.Left(StatusBarItemMaxLength - 5) + "...";

                return string.Format("[{0}]", orig);
            }
        }

        public string State
        {
            get { return _state; }
            private set
            {
                _state = value;
                OnPropertyChanged("State");
            }
        }

        public int PingStateIndex
        {
            get { return _pingStateIndex; }
            private set
            {
                _pingStateIndex = value;
                OnPropertyChanged("PingStateIndex");
            }
        }

        public string SystemMessage
        {
            get { return _systemMessage; }
            private set
            {
                _systemMessage = value;
                OnPropertyChanged("SystemMessage");
            }
        }

        public string SystemMessageCount
        {
            get { return _systemMessageCount; }
            private set
            {
                _systemMessageCount = value;
                OnPropertyChanged("SystemMessageCount");
            }
        }

        public string SystemMessageSubject
        {
            get { return _systemMessageSubject; }
            private set
            {
                _systemMessageSubject = value;
                OnPropertyChanged("SystemMessageSubject");
            }
        }

        public string SystemMessageTime
        {
            get { return _systemMessageTime; }
            private set
            {
                _systemMessageTime = value;
                OnPropertyChanged("SystemMessageTime");
            }
        }

        public int SysMsgState
        {
            get { return _sysMsgState; }
            private set
            {
                _sysMsgState = value;
                OnPropertyChanged("SysMsgState");
            }
        }

        private bool _waitIndicatorVisible;
        public bool WaitIndicatorVisible
        {
            get { return _waitIndicatorVisible; }
            set
            {
                if (_waitIndicatorVisible == value)
                    return;

                _waitIndicatorVisible = value;
                OnPropertyChanged("WaitIndicatorVisible");
            }
        }

        private string _sysEnvironmentInfo;
        public string SysEnvironmentInfo
        {
            get { return _sysEnvironmentInfo; }
            set
            {
                if (_sysEnvironmentInfo == value)
                    return;
                _sysEnvironmentInfo = value;
                OnPropertyChanged("SysEnvironmentInfo");
            }
        }

        private MenuTileViewModelBase _mainTileMenu;
        public MenuTileViewModelBase MainTileMenu
        {
            get { return _mainTileMenu; }
            set
            {
                if (Equals(_mainTileMenu, value))
                    return;
                _mainTileMenu = value;
                OnPropertyChanged("MainTileMenu");
            }
        }

        private MenuTileViewModelBase _ttaskMenu;
        public MenuTileViewModelBase TTaskMenu
        {
            get { return _ttaskMenu; }
            set
            {
                if (Equals(_ttaskMenu, value))
                    return;
                _ttaskMenu = value;
                OnPropertyChanged("TTaskMenu");
            }
        }

        private MenuTileViewModelBase _infoMenu;
        public MenuTileViewModelBase InfoMenu
        {
            get { return _infoMenu; }
            set
            {
                if (Equals(_infoMenu, value))
                    return;
                _infoMenu = value;
                OnPropertyChanged("InfoMenu");
            }
        }

        public bool HasCriticalErrors { get; private set; }

        public ICommand LoginCommand { get; private set; }
        public ICommand LogoffCommand { get; private set; }
        public ICommand SystemMessageCommand { get; private set; }
        public ICommand ShowSystemMessageCommand { get; private set; }
        public ICommand ClearCacheCommand { get; private set; }
        public ICommand MainMenuShowCommand { get; private set; }
        public ICommand SysDbInfoCommand { get; private set; }
        #endregion .  Properties  .

        #region .  Methods  .
        private void Logoff()
        {
            try
            {
                _inLogOff = true;
                var vs = IoC.Instance.Resolve<IViewService>();
                if (!vs.CloseAll(true))
                    throw new OperationException(Resources.StringResources.CantCloseSessionError);

                MainTileMenu = null;
                TTaskMenu = null;
                InfoMenu = null;

                AuthenticationHelper.LogOff();
            }
            finally
            {
                _inLogOff = false;
            }
        }

        private static bool CanReAuthenticate()
        {
            return true;
        }

        private async void Login()
        {
            Logoff();

            if (AuthenticationHelper.Authenticate())
            {
                try
                {
                    WaitIndicatorVisible = true;

                    // чистим кэш после перерисовки
                    await DoClearCacheAsync();

                    // перевычитывем главное меню
                    ShowMainMenu();
                }
                finally
                {
                    WaitIndicatorVisible = false;
                }
            }
        }

        public void ShowMainMenu()
        {
            var mtmenu = IoC.Instance.Resolve<MainMenuTileViewModel>();
            mtmenu.InitializeMenu();
            MainTileMenu = mtmenu;

            var taskmenu = IoC.Instance.Resolve<TTaskMenuViewModel>();
            taskmenu.InitializeMenu();
            TTaskMenu = taskmenu;

            var infomenu = IoC.Instance.Resolve<InfoMenuTileViewModel>();
            infomenu.InitializeMenu();
            InfoMenu = infomenu;
        }

#if DEBUG
#else
        private void CheckStatus(Object stateInfo)
        {
            if (_inLogOff || !AuthenticationHelper.IsAuthenticated)
                return;

            _pinger.Change(-1, DefaultPingTime);
            try
            {
                var sysMgr = IoC.Instance.Resolve<ISystemManager>();
                sysMgr.Ping();
                var pingTime = sysMgr.GetPingTime();
                State = string.Format("CONNECTED: ping time = {0}ms", pingTime);
                PingStateIndex = (pingTime <= GoodPing) ? 1 : 2;
            }
            catch
            {
                State = "DISCONNECTED";
                PingStateIndex = 3;
            }
            finally
            {
                _pinger.Change(DefaultPingTime, DefaultPingTime);
            }
        }
#endif
        private async void ClearCache()
        {
            try
            {
                WaitIndicatorVisible = true;
                await DoClearCacheAsync();
            }
            finally
            {
                WaitIndicatorVisible = false;
            }
        }

        private async Task DoClearCacheAsync()
        {
            var mgr = IoC.Instance.Resolve<ISysObjectManager>();

            await Task.Factory.StartNew(() =>
            {
                mgr.ClearCache();

                //Загрузим начальные кэши
                BLHelper.FillInitialCaches();
            });
        }

        private void ValidateTruck()
        {
            try
            {
                SplashScreenHelper.SetState(Resources.StringResources.StateValidateTruck);

                var mgrClient = IoC.Instance.Resolve<IBaseManager<Client>>();
                var clientcode = GetClientCode();
                var client = mgrClient.Get(clientcode, GetModeEnum.Partial);
                if (client == null)
                    throw new DeveloperException("Can't find Client by key '{0}'.", clientcode);

                if (string.IsNullOrEmpty(client.TruckCode_R))
                {
                    HasCriticalErrors = true;
                    _criticalErrorMessages.Add(string.Format(Resources.StringResources.TruckNotValidated,
                        GetClientCode()));
                }
                else
                {
                    WMSEnvironment.Instance.TruckCode = client.TruckCode_R;
                }
            }
            catch (Exception ex)
            {
                HasCriticalErrors = true;
                var msg = Resources.StringResources.TruckValidatedError;
                _criticalErrorMessages.Add(msg);
                ExceptionPolicy.Instance.HandleException(new OperationException(msg, ex), "RclBackgroundProcess");
            }
        }

        public string GetCriticalErrorMessage()
        {
            if (_criticalErrorMessages == null || !_criticalErrorMessages.Any())
                return Resources.StringResources.EmptyError;
            return string.Join(Environment.NewLine, _criticalErrorMessages);
        }

        private DbSysInfo GetSysDbInfo(bool notvalidatetruck = false)
        {
            if (WMSEnvironment.Instance.AuthenticatedUser != null)
            {
                if (!notvalidatetruck)
                    ValidateTruck();
                return WMSEnvironment.Instance.DbSystemInfo;
            }
            return null;
        }

        private void UpdateSysEnvironmentInfo(DbSysInfo sysdbinfo)
        {
            if (sysdbinfo == null)
            {
                SysEnvironmentInfo = SysDbInfoNone;
            }
            else
            {
                var version = string.IsNullOrEmpty(sysdbinfo.Version) ? SysDbInfoNone : sysdbinfo.Version;
                SysEnvironmentInfo = string.Format("{0}", version);
            }
            DispatcherHelper.BeginInvoke(new Action(ValidateWorker));
        }

        public void ValidateWorker()
        {
            SplashScreenHelper.SetState(Resources.StringResources.StateValidateWorker);
            if (WMSEnvironment.Instance.AuthenticatedUser == null)
            {
                HasCriticalErrors = true;
                _criticalErrorMessages.Add("User is null.");
                return;
            }

            // получим работника привязанного к пользователю
            WMSEnvironment.Instance.WorkerId = null;
            try
            {
                var currentUser = WMSEnvironment.Instance.AuthenticatedUser.GetSignature();
                using (var mgr = IoC.Instance.Resolve<IBaseManager<Worker>>())
                {
                    var filter = string.Format("USERCODE_R = '{0}'", currentUser);
                    var workers = mgr.GetFiltered(filter).ToArray();
                    if (workers.Length == 1)
                    {
                        WMSEnvironment.Instance.WorkerId = workers[0].GetKey<decimal>();
                    }
                    else if (workers.Length > 1)
                    {
                        WMSEnvironment.Instance.WorkerId = SelectWorker(filter);
                    }

                    if (!WMSEnvironment.Instance.WorkerId.HasValue)
                    {
                        HasCriticalErrors = true;
                        _criticalErrorMessages.Add(string.Format(Resources.StringResources.WorkerNotValidated, currentUser));
                    }
                    else
                    {
                        // Запишем работника в сессию
                        if (!WMSEnvironment.Instance.SessionId.HasValue)
                            return;

                        using (var mgrSession = (IClientSessionManager)IoC.Instance.Resolve<IBaseManager<ClientSession>>())
                        {
                            mgrSession.UpdateWorker(WMSEnvironment.Instance.SessionId.Value, WMSEnvironment.Instance.WorkerId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HasCriticalErrors = true;
                var msg = Resources.StringResources.WorkerValidatedError;
                _criticalErrorMessages.Add(msg);
                ExceptionPolicy.Instance.HandleException(new OperationException(msg, ex), "RclBackgroundProcess");
            }
        }

        private decimal? SelectWorker(string filter)
        {
            var workerModel = new DialogSourceViewModel
            {
                PanelCaption = Resources.StringResources.SelectWorkerPanelCaption,
                FontSize = 14,
                IsMenuVisible = false,
            };

            var lstWorkers = new ValueDataField
            {
                Name = "lstWorkers",
                LabelPosition = "Top",
                Caption = Resources.StringResources.WorkerList
            };
            lstWorkers.FieldName = lstWorkers.Name;
            lstWorkers.SourceName = lstWorkers.Name;
            lstWorkers.FieldType = typeof(Object);
            lstWorkers.LookupCode = "WORKER_WORKERFIO_RCL";
            lstWorkers.LookupFilterExt = filter;
            lstWorkers.Properties["MaxRowsOnPage"] = 6;
            lstWorkers.Properties["CloseDialogOnSelectedItemChanged"] = true;
            lstWorkers.Properties["LookupType"] = "SelectControl";
            lstWorkers.SetFocus = true;
            workerModel.Fields.Add(lstWorkers);
            workerModel.UpdateSource();

            var vs = IoC.Instance.Resolve<IViewService>();

            while (true)
            {
                if (vs.ShowDialogWindow(workerModel, false) != true)
                {
                    vs.ShowDialog(Resources.StringResources.RCL, Resources.StringResources.WorkerNotSelect, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    continue;
                }
                    
                switch (workerModel.MenuResult)
                {
                    case "Value":
                        var worker = workerModel["lstWorkers"];
                        decimal workerId;
                        if (!decimal.TryParse(worker.ToString(), out workerId))
                            throw new DeveloperException(string.Format("Bad workerId value '{0}'.",
                                worker));

                        return workerId;
                    default:
                        continue;
                }
            }
        }

        private void OnSysDbInfo()
        {
            var sysdbinfo = GetSysDbInfo(true);
            UpdateSysEnvironmentInfo(sysdbinfo);

            var environment = sysdbinfo == null ? SysDbInfoNone : (string.IsNullOrEmpty(sysdbinfo.Environment) ? SysDbInfoNone : sysdbinfo.Environment);
            var version = sysdbinfo == null ? SysDbInfoNone : (string.IsNullOrEmpty(sysdbinfo.Version) ? SysDbInfoNone : sysdbinfo.Version);
            var site = sysdbinfo == null ? SysDbInfoNone : (string.IsNullOrEmpty(sysdbinfo.Site) ? SysDbInfoNone : sysdbinfo.Site);

            var message = string.Format(Resources.StringResources.SysDbInfoMessage, environment, Environment.NewLine, version, site);
            var vs = IoC.Instance.Resolve<IViewService>();
            vs.ShowDialog(Resources.StringResources.SysDbInfoDialogTitle,
                message,
                MessageBoxButton.OK, MessageBoxImage.Information,
                MessageBoxResult.OK);
        }

        public static string GetClientCode()
        {
            var count = 0;
            while (string.IsNullOrEmpty(WMSEnvironment.Instance.ClientCode))
            {
                if (count > 200) //Ждем 1 мин.
                    throw new DeveloperException("Can't get ClientCode from WMSEnvironment.");
                Thread.Sleep(300);
                count++;
            }
            return WMSEnvironment.Instance.ClientCode;
        }
        #endregion .  Methods  .
    }
}