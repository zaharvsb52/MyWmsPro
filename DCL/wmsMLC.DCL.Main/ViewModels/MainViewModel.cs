using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Bars.Native;
using log4net;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Helpers;
using wmsMLC.DCL.Main.Properties;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.ViewModels;
using wmsMLC.General.Services;

namespace wmsMLC.DCL.Main.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region .  Fields & Consts  .

        private ILog _log = LogManager.GetLogger(typeof(MainViewModel));
        public const string UserInfoPropertyName = "UserInfo";
        public const string ShowMainMenuAction = "MainTreeMenu.Show";
        private const string SysDbInfoNone = "?";
        protected static int DefaultPingTime = 1000;
        private string _state;
        private int _pingStateIndex;
        protected static int GoodPing = 5; // хороший пинг
        private readonly Timer _pinger;
        private readonly IAuthenticationProvider _authenticationProvider;
        private string _systemMessage;
        private string _systemMessageSubject;
        private string _systemMessageTime;
        private string _systemMessageCount;
        private int _sysMsgState;
        private readonly IChatManager _chatManager;
        private bool _waitIndicatorVisible;
        private static volatile bool _inLogOff;
        private bool _systemUpdate;
        private string _workerFIO;
        
        private const string UpdatePrepareCmd = "/updateprepare";
        private const string UpdateCmd = "/update";
        private const string UpdateCompleteCmd = "/updatecomplete";
        private const string UpdateCancelCmd = "/updatecancel";
        #endregion .  Fields & Consts  .

        public MainViewModel()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;
            GoodPing = Settings.Default.GoodPingInMS;
            DefaultPingTime = Settings.Default.DefaultPingTimeInMS;

            _authenticationProvider = IoC.Instance.Resolve<IAuthenticationProvider>();
            _authenticationProvider.AuthenticatedUserChanged += (sender, args) =>
                    {
                        OnPropertyChanged(UserInfoPropertyName);
                        UpdateSysEnvironmentInfo(GetSysDbInfo());
                        WriteWorker(null);
                        if (WMSEnvironment.Instance.AuthenticatedUser != null && _chatManager != null)
                        {
                            new TaskFactory().StartNew(() =>
                            {
                                var signature = WMSEnvironment.Instance.AuthenticatedUser.GetSignature();
                                using (var mgr = IoC.Instance.Resolve<IBaseManager<UserGroup>>())
                                {
                                    var rooms = mgr.GetFiltered(string.Format("USERGROUPCODE in (select g.USERGROUPCODE_R from RUSER2GROUP g where g.USERCODE_R = '{0}')", signature),
                                        FilterHelper.GetAttrEntity<UserGroup>(UserGroup.UserGroupCodePropertyName, UserGroup.USERGROUPNAMEPropertyName)).
                                        ToDictionary(r => r.GetKey<string>(), r => r.GetProperty<string>(UserGroup.USERGROUPNAMEPropertyName));
                                    _chatManager.SetDefaultRooms(rooms);
                                }
                                _chatManager.Disconnect();
                                _chatManager.Connect(signature, signature);
                            });
                        }
                        GetWorkers();
                        SetMandantNames();
                    };

            // отображаем информацию о пользователе
            OnPropertyChanged(UserInfoPropertyName);

            // запускаем пинг
            _pinger = new Timer(CheckStatus, null, 0, DefaultPingTime);

            if (IoC.Instance.TryResolve<IChatManager>(out _chatManager))
            {
                _chatManager.Message += chatManager_Message;
                _chatManager.StateChanged += _chatManager_StateChanged;
            }

            PingStateIndex = 0;
            _sysMsgState = 0;

            AboutCommand = new DelegateCustomCommand(ShowAboutBox, () => true);
            LoginCommand = new DelegateCustomCommand(Login, CanReAuthenticate);
            LogoffCommand = new DelegateCustomCommand(Logoff, () => !SystemUpdate);
            SystemMessageCommand = new DelegateCustomCommand(CreateSysMsg, () => !SystemUpdate);
            //ClearCacheCommand = new DelegateCustomCommand(ClearCache, () => true);
            SysDbInfoCommand = new DelegateCustomCommand(OnSysDbInfo, () => true);
            LiteCacheClearCommand = new DelegateCustomCommand(LiteClearCache, () => !SystemUpdate);
            DashboardShowCommand = new DelegateCustomCommand(DashboardShow, () => !SystemUpdate);
            TopologyShowCommand = new DelegateCustomCommand(TopologyShow, () => !SystemUpdate);
            PrintReportCommand = new DelegateCustomCommand(PrintReport, () => !SystemUpdate);
            PropertyCommand = new DelegateCustomCommand(PropertyShow, () => !SystemUpdate);
            WorkingManageShowCommand = new DelegateCustomCommand(WorkingManageShow, () => !SystemUpdate);
            PackingShowCommand = new DelegateCustomCommand(PackingShow, () => !SystemUpdate);
            CustomsShowCommand = new DelegateCustomCommand(CustomsShow, () => !SystemUpdate);
            ChatShowCommand = new DelegateCustomCommand(ChatShow, CanChatShow);
            NextSDCLCommand = new DelegateCustomCommand(NextSDCL, () => !SystemUpdate);
            AddWorkerCommand = new DelegateCustomCommand(AddWorker);
            SetWorkerCommand = new DelegateCustomCommand<string>(SetWorker);
            MandantsCommand = new DelegateCustomCommand(ShowSelectMandant);
            
            SystemMessageSubject = StringResources.StateRun;
            SystemMessage = string.Format(StringResources.EnjoyYourWork, Environment.NewLine);

            InfoWorkers = new ObservableCollection<ItemInfo>();
        }

        void _chatManager_StateChanged(MsgState state)
        {
            switch (state)
            {
                case MsgState.None:
                    SysMsgState = 0;
                    break;
                case MsgState.Received:
                    SysMsgState = 1;
                    break;
                case MsgState.NotReaded:
                    SysMsgState = 2;
                    break;
                case MsgState.Readed:
                    SysMsgState = 3;
                    break;
            }
        }

        void chatManager_Message(string from, string message)
        {
            if (message.StartsWith(UpdatePrepareCmd))
            {
                DispatcherHelper.Invoke(new Action(() =>
                {
                    var customMsg = message.Substring(UpdatePrepareCmd.Length).Trim();
                    SystemMessageSubject = "ВНИМАНИЕ!";
                    SystemMessage =
                        string.Format(
                            "Через {0} планируется обновление системы.\r\nРекомендуется завершить все работы и выйти из системы.\r\nИначе приложение будет перезапущено автоматически.",
                            string.IsNullOrEmpty(customMsg) ? "5 мин." : customMsg);
                    var vs = IoC.Instance.Resolve<IViewService>();
                    vs.ShowDialog(SystemMessageSubject, SystemMessage, MessageBoxButton.OK,
                        MessageBoxImage.Exclamation, MessageBoxResult.OK);
                }));
            }
            if (message == UpdateCmd)
            {
                // администратор что-то напутал - обновление уже идет
                if (SystemUpdate)
                    return;
                SystemUpdate = true;
                DispatcherHelper.Invoke(new Action(() =>
                {
                    Logoff();
                    SystemMessageSubject = "ВНИМАНИЕ!";
                    SystemMessage =
                        "Выполняется обновление системы.\r\nПосле окончания процесса приложение будет перезапущено.";
                }));
            }
            if (message == UpdateCompleteCmd)
            {
                // администратор что-то напутал - небыло обновления
                if (!SystemUpdate)
                    return;
                SystemUpdate = false;
                DispatcherHelper.Invoke(new Action(() =>
                {
                    SystemMessageSubject = "ВНИМАНИЕ!";
                    SystemMessage = "Обновление завершено.\r\nДля применения изменений приложение будет перезапущено автоматически.";
                    var vs = IoC.Instance.Resolve<IViewService>();
                    vs.ShowDialog(SystemMessageSubject, SystemMessage, MessageBoxButton.OK,
                        MessageBoxImage.Exclamation, MessageBoxResult.OK);
                    var procName = System.Reflection.Assembly.GetEntryAssembly().Location.Replace(".vshost", string.Empty);
                    _log.DebugFormat("Try to start new process '{0}'", procName);
                    Process.Start(procName);
                    Application.Current.Shutdown();
                }));
            }
            if (message == UpdateCancelCmd)
            {
                // администратор что-то напутал - небыло обновления
                if (!SystemUpdate)
                    return;
                SystemUpdate = false;
                DispatcherHelper.Invoke(new Action(() =>
                {
                    SystemMessageSubject = "ВНИМАНИЕ!";
                    SystemMessage = "Обновление системы отменено.\r\nВы можете продолжить работу.";
                    Login();
                }));
            }
        }

        #region .  Properties  .

        public ObservableCollection<ItemInfo> InfoWorkers { get; set; }

        public bool SystemUpdate
        {
            get { return _systemUpdate; }
            set
            {
                _systemUpdate = value;
                OnPropertyChanged("SystemUpdate");
            }
        }

        public string UserInfo
        {
            get
            {
                return _authenticationProvider == null || _authenticationProvider.AuthenticatedUser == null
                    ? StringResources.MainViewModel_Login
                    : string.Format(StringResources.MainViewModel_Logoff_Template,
                        _authenticationProvider.AuthenticatedUser.GetSignature());
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

        private string _mandantNames;
        public string MandantNames
        {
            get { return _mandantNames; }
            set
            {
                _mandantNames = value;
                OnPropertyChanged("MandantNames");
            }
        }

        public string WorkerFIO
        {
            get { return _workerFIO; }
            set
            {
                if (_workerFIO == value)
                    return;
                _workerFIO = value;
                OnPropertyChanged("WorkerFIO");
            }
        }

        public ICommand AboutCommand { get; private set; }
        public ICommand LoginCommand { get; private set; }
        public ICommand LogoffCommand { get; private set; }
        public ICommand SystemMessageCommand { get; private set; }
        public ICommand ClearCacheCommand { get; private set; }
        public ICommand MainMenuShowCommand { get; private set; }
        public ICommand SysDbInfoCommand { get; private set; }
        public ICommand LiteCacheClearCommand { get; private set; }
        public ICommand DashboardShowCommand { get; private set; }
        public ICommand TopologyShowCommand { get; private set; }
        public ICommand WorkingManageShowCommand { get; private set; }
        public ICommand PrintReportCommand { get; private set; }
        public ICommand PropertyCommand { get; private set; }
        public ICommand PackingShowCommand { get; private set; }
        public ICommand CustomsShowCommand { get; private set; }
        public ICommand ChatShowCommand { get; private set; }
        public ICommand NextSDCLCommand { get; private set; }
        public ICommand AddWorkerCommand { get; private set; }
        public ICommand SetWorkerCommand { get; private set; }
        public ICommand MandantsCommand { get; private set; }
        
        #endregion .  Properties  .

        #region .  Methods  .
        private static void Logoff()
        {
            try
            {
                _inLogOff = true;
                var vs = IoC.Instance.Resolve<IViewService>();
                if (!vs.CloseAll(true))
                    throw new OperationException(StringResources.CantCloseSessionError);

                SignalRHelper.DisconnectFromServer();
                AuthenticationHelper.LogOff();
            }
            finally
            {
                _inLogOff = false;
            }
        }

        private bool CanReAuthenticate()
        {
            return !SystemUpdate;
        }

        private async void Login()
        {
            Logoff();

            if (AuthenticationHelper.Authenticate())
            {
                try
                {
                    SignalRHelper.TryConnectToServer();
                    WaitIndicatorVisible = true;

                    // чистим кэш после перерисовки
                    await DoLiteClearCacheAsync();

                    // перевычитывем главное меню
                    LoadCustomization();
                    ShowTree();
                }
                finally
                {
                    WaitIndicatorVisible = false;
                }
            }
        }

        private void GetWorkers()
        {
            WMSEnvironment.Instance.WorkerId = null;

            var sign = GetUserCode(false);
            if (sign == null)
                return;

            InfoWorkers.Clear();
            InfoWorkers.Add(new ItemInfo() {Caption = StringResources.Add, ItemCommand = AddWorkerCommand});

            List<Worker> workers;

            using (var mgr = IoC.Instance.Resolve<IBaseManager<Worker>>())
            {
                var workerFilter = string.Format("USERCODE_R = '{0}'", sign);
                workers = mgr.GetFiltered(workerFilter, GetModeEnum.Partial).ToList();
            }

            switch (workers.Count)
            {
                case 0:
                    return;
                case 1:
                    WriteWorker(workers[0], false);
                    break;
                default:
                    if (Settings.Default.WorkerID != null)
                    {
                        var selectWorker = workers.FirstOrDefault(w => w.GetKey().Equals(Settings.Default.WorkerID));
                        if (selectWorker != null)
                            WriteWorker(selectWorker, false);
                    }
                    break;
            }

            // Создаем коллекцию Items для меню
            foreach (var w in workers)
                InfoWorkers.Add(new ItemInfo() {Caption = w.WorkerFIO, ItemCommand = SetWorkerCommand});
        }

        private void SetWorker(string FIO)
        {
            WMSEnvironment.Instance.WorkerId = null;

            var sign = GetUserCode();
            if (string.IsNullOrEmpty(sign))
                return;

            using (var mgr = IoC.Instance.Resolve<IBaseManager<Worker>>())
            {
                var workers = mgr.GetFiltered(string.Format("USERCODE_R = '{0}'", WMSEnvironment.Instance.AuthenticatedUser.GetSignature()), GetModeEnum.Partial).ToList();
                if (workers.Count == 0)
                {
                    var vs = IoC.Instance.Resolve<IViewService>();
                    vs.ShowDialog(StringResources.Error, string.Format(StringResources.ErrorSelectWorkerByUser, sign), MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.OK);
                    return;
                }
                var worker = workers.FirstOrDefault(w => FIO.Equals(w.WorkerFIO));
                if (worker == null)
                    return;

                WriteWorker(worker);
            }
        }

        private void AddWorker()
        {
            WMSEnvironment.Instance.WorkerId = null;
            var sign = GetUserCode();

            if (string.IsNullOrEmpty(sign))
                return;

            var vm = new ObjectListViewModelBase<Worker>
            {
                Mode = ObjectListMode.LookUpList3Points,
                AllowAddNew = true,
                PanelCaption = StringResources.SelectWorker,
                IsActive = true
            };
            vm.InitializeMenus();
            vm.ApplyFilter("USERCODE_R is null");
            var window = new Views.CustomLookUpOptPopupContent {DataContext = vm};

            if (window.Owner == null && Application.Current.MainWindow.IsActive)
                window.Owner = Application.Current.MainWindow;

            if (window.ShowDialog() != true)
                return;

            var worker = vm.SelectedItem as Worker;
            if (worker == null)
                return;

            if (!string.IsNullOrEmpty(worker.UserCode) && worker.UserCode != sign)
            {
                var vs = IoC.Instance.Resolve<IViewService>();
                vs.ShowDialog(StringResources.Error, StringResources.ErrorSelectWorker, MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.OK);
                return;
            }

            if (!string.IsNullOrEmpty(worker.UserCode))
            {
                WriteWorker(worker, false);
                return;
            }

            using (var mgr = IoC.Instance.Resolve<IBaseManager<Worker>>())
            {
                worker.UserCode = sign;
                mgr.Update(worker);
            }
            InfoWorkers.Add(new ItemInfo() { Caption = worker.WorkerFIO, ItemCommand = AddWorkerCommand });
            WriteWorker(worker);
        }

        private void WriteWorker(Worker worker, bool isChangeDefault = true)
        {
            if (worker != null)
            {
                WMSEnvironment.Instance.WorkerId = (decimal?)worker.GetKey();
                WorkerFIO = worker.WorkerFIO;

                if (isChangeDefault)
                {
                    Settings.Default.WorkerID = WMSEnvironment.Instance.WorkerId;
                    Settings.Default.Save();
                }
            }
            else
            {
                WMSEnvironment.Instance.WorkerId = null;
                WorkerFIO = null;
            }

            // Запишем работника в сессию
            if (!WMSEnvironment.Instance.SessionId.HasValue)
                return;

            using (var mgrSession = (IClientSessionManager)IoC.Instance.Resolve<IBaseManager<ClientSession>>())
            {
                mgrSession.UpdateWorker(WMSEnvironment.Instance.SessionId.Value, WMSEnvironment.Instance.WorkerId);
            }
        }

        private string GetUserCode(bool isView = true)
        {
            if (WMSEnvironment.Instance.AuthenticatedUser != null && WMSEnvironment.Instance.AuthenticatedUser.GetSignature() != null)
                return WMSEnvironment.Instance.AuthenticatedUser.GetSignature();

            if (isView)
            {
                var vs = IoC.Instance.Resolve<IViewService>();
                vs.ShowDialog(StringResources.Error, StringResources.UserCanNotBeIdentified, MessageBoxButton.OK, MessageBoxImage.Hand, MessageBoxResult.OK);
            }
            return null;
        }
        private void SetMandantNames()
        {
            var user = WMSEnvironment.Instance.AuthenticatedUser;
            if (user == null)
                return;

            using (var mgr = IoC.Instance.Resolve<IBaseManager<User2Mandant>>())
            {
                var mandants = mgr.GetFiltered(string.Format("USERCODE_R = '{0}'", WMSEnvironment.Instance.AuthenticatedUser.GetSignature())).ToArray();
                if (!mandants.Any())
                    return;

                Array.Sort(mandants, (x, y) => string.Compare(x.MandantCode, y.MandantCode, StringComparison.Ordinal));

                var actMandants = mandants.Where(m => m.User2MandantIsActive).ToArray();
                var name = actMandants.Any() ? string.Join(",", actMandants.Select(m => m.MandantCode)) : string.Empty;
                MandantNames = name.Length > 15 ? string.Format("{0}...", name.Substring(0, 15)) : name;
            }
        }

        private void ShowSelectMandant()
        {
            var vs = IoC.Instance.Resolve<IViewService>();
            var model = new SelectActiveMandantViewModel();
            vs.ShowDialogWindow(model, true, true);
            if (!model.IsSave) return;

            SetMandantNames();
            LiteClearCache();

            var mgr = IoC.Instance.Resolve<IBaseManager<Mandant>>();
            mgr.RiseManagerChanged( NotifyCollectionChangedAction.Add, null );

            //var vsMess = IoC.Instance.Resolve<IViewService>();
            //vsMess.ShowDialog(StringResources.Information, StringResources.UpdateLayoutInfo,
            //    MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
        }

        public static void LoadCustomization()
        {
            var vs = IoC.Instance.Resolve<IViewService>();
            vs.AllRestoreLayout();
        }

        public static void ShowTree()
        {
            if (!ConnectionManager.Instance.AllowRequest())
                return;
            var vs = IoC.Instance.Resolve<IViewService>();
            vs.Show(ShowMainMenuAction, new ShowContext { DockingType = DockType.Left });
        }

        public static void LoadDxAssemblies()
        {
            //Регистрируем dx декораторы для BarManager'а
            //Регистрируем наш ICustomizationService и CustomizationDecorator для отображения формы настройки
            BarNameScope.RegisterDecorator<CustomizationDecorator, ICustomizationService>(
                () => new CustomCustomizationDecorator(), p => (ICustomizationService)new CustomCustomizationService(p),
                () => (ICustomizationService)new CustomCustomizationService(null));

            //Регистрируем для исправления баг с горячими клавишами
            BarNameScope.RegisterDecorator<ItemCommandSourceStrategy, ICommandSourceService>(
                () => new CustomItemCommandSourceStrategy(),
                p => new CustomCommandSourceService((CustomItemCommandSourceStrategy) p),
                () => new CustomCommandSourceService());

            var vs = IoC.Instance.Resolve<IViewService>();
            LoadDxAssemblies<IWB>(vs);
            LoadDxAssemblies<OWB>(vs);
        }

        private static void LoadDxAssemblies<T>(IViewService vs) where T : WMSBusinessObject
        {
            var vm = IoC.Instance.Resolve<IObjectViewModel<T>>();
            vm.SetSource(Activator.CreateInstance<T>());
            var view = vs.GetView(vm);
            vs.AddViewToCache(vm.GetType(), view);
            //var disposeview = view as IDisposable;
            //if (disposeview != null)
            //    disposeview.Dispose();
        }

        private void ShowAboutBox()
        {
            var vs = IoC.Instance.Resolve<IViewService>();
            var model = new AboutBoxViewModel();
            vs.ShowDialogWindow(model, true, true);
        }

        private void CreateSysMsg()
        {
            if (!ConnectionManager.Instance.AllowRequest())
                return;
            var vs = IoC.Instance.Resolve<IViewService>();
            var vm = new SystemMessageViewModel();
            if (vs.ShowDialogWindow(vm, true, false, "30%", "20%") != true) return;
            if (_chatManager != null)
                _chatManager.SendMessage(vm.User, vm.Message);
        }

        private void CheckStatus(Object stateInfo)
        {
            if (_inLogOff || !AuthenticationHelper.IsAuthenticated)
                return;
            try
            {
                _pinger.Change(-1, DefaultPingTime);
                if (WMSEnvironment.Instance.IsConnected == null)
                {
                    State = "Отключен";
                    PingStateIndex = 2;
                } 
                else if (WMSEnvironment.Instance.IsConnected == true)
                {
                    State = "Подключен";
                    PingStateIndex = 1;
                }
                else
                {
                    State = "Нет связи";
                    PingStateIndex = 3;
                }
            }
            finally
            {
                _pinger.Change(DefaultPingTime, DefaultPingTime);
            }
        }

        //private async void ClearCache()
        //{
        //    if (!ConnectionManager.Instance.AllowRequest())
        //        return;
        //    try
        //    {
        //        WaitIndicatorVisible = true;
        //        await DoClearCacheAsync();
        //    }
        //    finally
        //    {
        //        WaitIndicatorVisible = false;
        //    }
        //}

        private async void LiteClearCache()
        {
           try
            {
                WaitIndicatorVisible = true;
                await DoLiteClearCacheAsync();
            }
            finally
            {
                WaitIndicatorVisible = false;
            }
        }

        private void PackingShow()
        {
            var vs = IoC.Instance.Resolve<IViewService>();
            vs.Show(StringResources.Packing, new ShowContext { DockingType = DockType.Document });
        }

        private void CustomsShow()
        {
            var vs = IoC.Instance.Resolve<IViewService>();
            vs.Show(DclModules.Customs, new ShowContext { DockingType = DockType.Document });
        }

        private void DashboardShow()
        {
            if (!ConnectionManager.Instance.AllowRequest())
                return;
            var vs = IoC.Instance.Resolve<IViewService>();
            vs.Show(new DashboardViewModel(), new ShowContext { DockingType = DockType.Document });
        }

        private void PrintReport()
        {
            if (!ConnectionManager.Instance.AllowRequest())
                return;
            var vs = IoC.Instance.Resolve<IViewService>();
            var ovm = new PrintViewModel(new object[0]);
            vs.ShowDialogWindow(viewModel: ovm, isRestoredLayout: true, isNotNeededClosingOnOkResult: true);
        }

        private void PropertyShow()
        {
            if (!ConnectionManager.Instance.AllowRequest())
                return;
            var vs = IoC.Instance.Resolve<IViewService>();
            var ovm = new PropertyViewModel();
            ovm.GetData();
            vs.Show(ovm, new ShowContext { DockingType = DockType.Document });
            if (!ovm.CanSaveProperty()) 
                return;

            if (vs.ShowDialog(StringResources.PropertyWindow, StringResources.ConfirmationUnsavedData, 
                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) ==
                MessageBoxResult.Yes)
            {
                ovm.SaveProperty();
            }
        }

        private void TopologyShow()
        {
            if (!ConnectionManager.Instance.AllowRequest())
                return;
            var vs = IoC.Instance.Resolve<IViewService>();
            var vm = IoC.Instance.Resolve<ITopologyViewModel>();
            vs.Show(vm, new ShowContext { DockingType = DockType.Document });
        }

        private void WorkingManageShow()
        {
            if (!ConnectionManager.Instance.AllowRequest())
                return;
            var vs = IoC.Instance.Resolve<IViewService>();
            var vm = IoC.Instance.Resolve<IWorkingManageViewModel>();
            vs.Show(vm, new ShowContext { DockingType = DockType.Document });
        }

        private bool CanChatShow()
        {
            return _chatManager != null && !SystemUpdate;
        }

        private void ChatShow()
        {
            var vs = IoC.Instance.Resolve<IViewService>();
            vs.Show(StringResources.Chat, new ShowContext { DockingType = DockType.Right });
        }

        //private async Task DoClearCacheAsync()
        //{
        //    var mgr = IoC.Instance.Resolve<ISysObjectManager>();

        //    await Task.Factory.StartNew(() =>
        //        {
        //            mgr.ClearCache();

        //            //Загрузим начальные кэши
        //            BLHelper.FillInitialCaches();
        //        });
        //}

        private async Task DoLiteClearCacheAsync()
        {
            var mgr = IoC.Instance.Resolve<ISysObjectManager>();
            //await Task.Factory.StartNew(CacheAspect.ClearCache);
            await Task.Factory.StartNew(mgr.LiteClearCache);
        }

        private DbSysInfo GetSysDbInfo()
        {
            return WMSEnvironment.Instance.AuthenticatedUser == null ? null : WMSEnvironment.Instance.DbSystemInfo;
        }

        private void UpdateSysEnvironmentInfo(DbSysInfo sysdbinfo)
        {
            if (sysdbinfo == null)
            {
                SysEnvironmentInfo = SysDbInfoNone;
                return;
            }

            var sysEnvironment = string.IsNullOrEmpty(sysdbinfo.Environment) ? (string.IsNullOrEmpty(sysdbinfo.Version) ? SysDbInfoNone : null) : sysdbinfo.Environment;
            SysEnvironmentInfo = string.Format("{0} {1}", sysEnvironment, sysdbinfo.Version).Trim();
        }

        private void OnSysDbInfo()
        {
            var sysdbinfo = GetSysDbInfo();
            UpdateSysEnvironmentInfo(sysdbinfo);
            
            var environment = sysdbinfo == null ? SysDbInfoNone : (string.IsNullOrEmpty(sysdbinfo.Environment) ? SysDbInfoNone : sysdbinfo.Environment);
            var version = sysdbinfo == null ? SysDbInfoNone : (string.IsNullOrEmpty(sysdbinfo.Version) ? SysDbInfoNone : sysdbinfo.Version);
            var site = sysdbinfo == null ? SysDbInfoNone : (string.IsNullOrEmpty(sysdbinfo.Site) ? SysDbInfoNone : sysdbinfo.Site);
            var sdcl = WMSEnvironment.Instance.SdclCode;
            var url = WMSEnvironment.Instance.EndPoint;

            var message = string.Format(StringResources.SysDbInfoMessage, Environment.NewLine, environment, version, site, sdcl, url);
            var vs = IoC.Instance.Resolve<IViewService>();
            vs.ShowDialog(StringResources.SysDbInfoDialogTitle,
                message,
                MessageBoxButton.OK, MessageBoxImage.Information,
                MessageBoxResult.OK);
        }

        private void NextSDCL()
        {
            var sc = IoC.Instance.Resolve<IServiceClient>();
            sc.Reconnect(null);
        }
        #endregion .  Methods  .
    }

    public class ItemInfo
    {
        public string Caption { get; set; }
        public ICommand ItemCommand { get; set; }
    }
}
