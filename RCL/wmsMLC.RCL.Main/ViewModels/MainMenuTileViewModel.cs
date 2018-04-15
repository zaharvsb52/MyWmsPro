using System;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Media;
using MLC.WebClient;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Components.Helpers;

namespace wmsMLC.RCL.Main.ViewModels
{
    public class MainMenuTileViewModel : MenuTileViewModelBase
    {
        public const string TileMenuActionHomeMenu = "HomeMenu";
        public const string TileMenuActionTransportTask = "TransportTask";
        public const string TileMenuActionPickingList = "PickingList";
        public const string TileMenuActionRefresh = "Refresh";
        public const string TileMenuActionInventory = "Inventory";
        public const string TileMenuActionWork = "Work";
        public const string TileMenuActionMandant = "Mandant";

        private readonly WmsAPI _api;

        public MainMenuTileViewModel()
        {
            _api = IoC.Instance.Resolve<WmsAPI>();
        }

        #region .  Properties  .

        private bool _statusVisible;
        public bool StatusVisible
        {
            get { return _statusVisible; }
            set
            {
                if (_statusVisible == value)
                    return;
                _statusVisible = value;
                OnPropertyChanged("StatusVisible");
            }
        }

        private string _tileMainMenuHeader;
        public string TileMainMenuHeader
        {
            get { return _tileMainMenuHeader; }
            set
            {
                if (_tileMainMenuHeader == value)
                    return;
                _tileMainMenuHeader = value;
                OnPropertyChanged("TileMainMenuHeader");
            }
        }

        private string _tileTransportTaskHeader;
        public string TileTransportTaskHeader
        {
            get { return _tileTransportTaskHeader; }
            set
            {
                if (_tileTransportTaskHeader == value)
                    return;
                _tileTransportTaskHeader = value;
                OnPropertyChanged("TileTransportTaskHeader");
            }
        }

        private string _tileTransportTaskTitle;
        public string TileTransportTaskTitle
        {
            get { return _tileTransportTaskTitle; }
            set
            {
                if (_tileTransportTaskTitle == value)
                    return;
                _tileTransportTaskTitle = value;
                OnPropertyChanged("TileTransportTaskTitle");
            }
        }

        private Brush _tileTransportTaskBackground;
        public Brush TileTransportTaskBackground
        {
            get { return _tileTransportTaskBackground; }
            set
            {
                if (Equals(_tileTransportTaskBackground, value))
                    return;
                _tileTransportTaskBackground = value;
                OnPropertyChanged("TileTransportTaskBackground");
            }
        }

        private string _tilePickingLisHeader;
        public string TilePickingLisHeader
        {
            get { return _tilePickingLisHeader; }
            set
            {
                if (_tilePickingLisHeader == value)
                    return;
                _tilePickingLisHeader = value;
                OnPropertyChanged("TilePickingLisHeader");
            }
        }

        private string _tilePickingLisTitle;
        public string TilePickingLisTitle
        {
            get { return _tilePickingLisTitle; }
            set
            {
                if (_tilePickingLisTitle == value)
                    return;
                _tilePickingLisTitle = value;
                OnPropertyChanged("TilePickingLisTitle");
            }
        }

        private Brush _tilePickingListBackground;
        public Brush TilePickingListBackground
        {
            get { return _tilePickingListBackground; }
            set
            {
                if (Equals(_tilePickingListBackground, value))
                    return;
                _tilePickingListBackground = value;
                OnPropertyChanged("TilePickingListBackground");
            }
        }

        private string _tileWorkHeader;
        public string TileWorkHeader
        {
            get { return _tileWorkHeader; }
            set
            {
                if (_tileWorkHeader == value)
                    return;
                _tileWorkHeader = value;
                OnPropertyChanged("TileWorkHeader");
            }
        }

        private string _tileRefreshHeader;
        public string TileRefreshHeader
        {
            get { return _tileRefreshHeader; }
            set
            {
                if (_tileRefreshHeader == value)
                    return;
                _tileRefreshHeader = value;
                OnPropertyChanged("TileRefreshHeader");
            }
        }

        private TimeSpan _tileRefreshContentChangeInterval;
        public TimeSpan TileRefreshContentChangeInterval
        {
            get { return _tileRefreshContentChangeInterval; }
            set
            {
                if (_tileRefreshContentChangeInterval == value)
                    return;
                _tileRefreshContentChangeInterval = value;
                OnPropertyChanged("TileRefreshContentChangeInterval");
            }
        }

        private Brush _tileRefreshBackground;
        public Brush TileRefreshBackground
        {
            get { return _tileRefreshBackground; }
            set
            {
                if (Equals(_tileRefreshBackground, value))
                    return;
                _tileRefreshBackground = value;
                OnPropertyChanged("TileRefreshBackground");
            }
        }

        private Brush _tileRefreshBackgroundWait;
        public Brush TileRefreshBackgroundWait
        {
            get { return _tileRefreshBackgroundWait; }
            set
            {
                if (Equals(_tileRefreshBackgroundWait, value))
                    return;
                _tileRefreshBackgroundWait = value;
                OnPropertyChanged("TileRefreshBackgroundWait");
            }
        }

        private bool _tileRefreshIsEnabled;
        public bool TileRefreshIsEnabled
        {
            get { return _tileRefreshIsEnabled; }
            set
            {
                if (_tileRefreshIsEnabled == value)
                    return;
                _tileRefreshIsEnabled = value;
                OnPropertyChanged("TileRefreshIsEnabled");
            }
        }

        private string _tileInfoHeader;
        public string TileInfoHeader
        {
            get { return _tileInfoHeader; }
            set
            {
                if (_tileInfoHeader == value)
                    return;
                _tileInfoHeader = value;
                OnPropertyChanged("TileInfoHeader");
            }
        }

        private string _tileInventoryHeader;
        public string TileInventoryHeader
        {
            get { return _tileInventoryHeader; }
            set
            {
                if (_tileInventoryHeader == value)
                    return;
                _tileInventoryHeader = value;
                OnPropertyChanged("TileInventoryHeader");
            }
        }

        private string _tileMandantHeader;
        public string TileMandantHeader
        {
            get { return _tileMandantHeader; }
            set
            {
                if (_tileMandantHeader == value)
                    return;
                _tileMandantHeader = value;
                OnPropertyChanged("TileMandantHeader");
            }
        }

        private string _tileMandantTitle;
        public string TileMandantTitle
        {
            get { return _tileMandantTitle; }
            set
            {
                if (_tileMandantTitle == value)
                    return;
                _tileMandantTitle = value;
                OnPropertyChanged("TileMandantTitle");
            }
        }

        private Brush _tileMandantBackground;
        public Brush TileMandantBackground
        {
            get { return _tileMandantBackground; }
            set
            {
                if (Equals(_tileMandantBackground, value))
                    return;
                _tileMandantBackground = value;
                OnPropertyChanged("TileMandantBackground");
            }
        }


        protected override Action DefaultCompletedActionHandler
        {
            get { return UpdateItems; }
        }

        #endregion .  Properties  .

        #region .  Methods  .
        #region . Menu .
        public override void InitializeMenu()
        {
            TileMainMenuHeader = Resources.StringResources.Menu;

            TileTransportTaskHeader = Resources.StringResources.MainTileMenuTransportTaskHeader;
            TileTransportTaskTitle = string.Empty;
            TileTransportTaskBackground = Application.Current.Resources[StyleKeys.TileBackgroundDefaultKey] as Brush;

            TilePickingLisHeader = string.Format(Resources.StringResources.MainTileMenuPickingLisHeader, string.Empty);
            TilePickingLisTitle = string.Empty;
            TilePickingListBackground = Application.Current.Resources[StyleKeys.TileBackgroundDefaultKey] as Brush;

            TileInfoHeader = Resources.StringResources.Info;
            TileInventoryHeader = Resources.StringResources.INV;

            TileWorkHeader = Resources.StringResources.Work;

            RefreshTileDefault();
        }
        #endregion . Menu .

        #region . Commands .

        protected override void OnMenuClick(string parameter)
        {
            if (!CanMenuClick(parameter))
                return;

            switch (parameter)
            {
                case TileMenuActionHomeMenu:
                    ShowHomeMenu();
                    break;
                case TileMenuActionTransportTask:
                    ShowTransportTaskMenu();
                    break;
                case TileMenuActionPickingList:
                    RunProcess("RclPickingMainMenu");
                    break;
                case TileMenuActionWork:
                    RunProcess("RCLWORKMAINMENU");
                    break;
                case TileMenuActionRefresh:
                    UpdateItems();
                    break;
                case TileMenuActionInventory:
                    RunProcess("RCLINVMAIN");
                    break;
                case TileMenuActionMandant:
                    RunProcess("RCLSELECTMANDANT");
                    break;
            }
        }

        #endregion . Commands .

        private async void ShowHomeMenu()
        {
            //var bpContext = new BpContext();
            //BPProcessManager.RunByCode("RCLMAINMENU", ref bpContext);

            try
            {
                WaitIndicatorVisible = true;
                var context = new BpContext();
                context.Properties.Add("IsMenuAboutVisible", true);

#if DEBUG
                context.Properties.Add("IsTestMenuVisible", true);
#else
                context.Properties.Add("IsTestMenuVisible", Properties.Settings.Default.IsTestMenuVisible);
#endif

                await RunBpAsync("RCLMAINMENU", context, DefaultCompletedHandler);
            }
            catch (Exception)
            {
                WaitIndicatorVisible = false;
                throw;
            }
        }

        private async void ShowTransportTaskMenu()
        {
            try
            {
                WaitIndicatorVisible = true;
                var context = new BpContext();
                context.Properties.Add("MenuCommandId", 0);
                context.Properties.Add("Restart", true);
                await RunBpAsync("RclTransportTaskNotification", context, DefaultCompletedHandler);
            }
            catch (Exception)
            {
                WaitIndicatorVisible = false;
                throw;
            }
        }

        private async void RunProcess(string code)
        {
            try
            {
                WaitIndicatorVisible = true;
                await RunBpAsync(code, null, DefaultCompletedHandler);
            }
            catch (Exception)
            {
                WaitIndicatorVisible = false;
                throw;
            }
        }

        /// <summary>
        /// Обновление главного меню. Небезопасный код, т.к. изменяем визуальную состовляющую. При вызове из другого потока необходимо использовать DispatcherHelper.Invoke().
        /// </summary>
        private async void UpdateItems()
        {
            try
            {
                RefreshTileUpdate(true);
                //PickingListUpdate();
                //await UpdateItemsAsync();

                if (!WMSEnvironment.Instance.SessionId.HasValue)
                    throw new Exception("SessionId is null");

                var transportTasksByTypeTask = _api.GetAvailableTransportTaskCountAsync(Convert.ToInt32(WMSEnvironment.Instance.SessionId.Value));
                var pickListCountTask = _api.GetAvailablePickListCountAsync(WMSEnvironment.Instance.TruckCode);

                await Task.WhenAll(transportTasksByTypeTask, pickListCountTask);

                var transportTasksByType = transportTasksByTypeTask.Result;
                var pickListCount = pickListCountTask.Result;

                var transportTaskCount = transportTasksByType.Values.Sum();
                TileTransportTaskTitle = transportTaskCount > 0 ? transportTaskCount.ToString() : string.Empty;
                TileTransportTaskBackground = Application.Current.Resources[transportTaskCount > 0
                    ? StyleKeys.TileBackgroundOrangeKey
                    : StyleKeys.TileBackgroundDefaultKey] as Brush;

                TilePickingLisTitle = pickListCount > 0 ? pickListCount.ToString() : string.Empty;
                TilePickingListBackground = Application.Current.Resources[
                    pickListCount > 0
                        ? StyleKeys.TileBackgroundOrangeKey
                        : StyleKeys.TileBackgroundDefaultKey] as Brush;

                RefreshMandant();
            }
            catch (Exception ex)
            {
                if (!ExceptionHandler(ex, Resources.ExceptionResources.TileMenuRefreshError))
                    throw;
            }
            finally
            {
                RefreshTileUpdate(false);
            }
        }

        private void RefreshTileUpdate(bool iswait)
        {
            TileRefreshIsEnabled = !iswait;
            if (iswait)
            {
                TileRefreshContentChangeInterval = TimeSpan.FromSeconds(2);
                TileRefreshHeader = Resources.StringResources.WaitShort;
                TileRefreshBackground =
                    TileRefreshBackgroundWait = Application.Current.Resources[StyleKeys.TileBackgroundWaitKey] as Brush;
            }
            else
            {
                RefreshTileDefault();
            }
        }

        private void RefreshTileDefault()
        {
            TileRefreshHeader = Resources.StringResources.MainTileMenuRefreshHeader;
            TileRefreshContentChangeInterval = MaxContentChangeInterval;
            TileRefreshBackground =
                TileRefreshBackgroundWait = Application.Current.Resources[StyleKeys.TileBackgroundGreenKey] as Brush;
        }

        private async void RefreshMandant()
        {
            var mgr = IoC.Instance.Resolve<IBaseManager<User2Mandant>>();
            var u2m = mgr.GetFiltered(string.Format("USERCODE_R = '{0}' and {1} = 1", WMSEnvironment.Instance.AuthenticatedUser.GetSignature(), User2Mandant.User2MandantIsActivePropertyName)).ToArray();
            if (!u2m.Any())
            {
                TileMandantHeader = 
                    TileMandantTitle = string.Empty;
                TileMandantBackground = Application.Current.Resources[StyleKeys.TileBackgroundDefaultKey] as Brush;
                return;
            }
            Array.Sort(u2m, (x, y) => string.Compare(x.MandantCode, y.MandantCode, StringComparison.Ordinal));
            TileMandantTitle = u2m.Count().ToString();
            var header = string.Join(",", u2m.Select(m => m.MandantCode));
            TileMandantHeader = u2m.Count() > 2 ? string.Format("{0}...", header.Substring(0, 7)) : header;
            TileMandantTitle = u2m.Count().ToString();
            TileMandantBackground = Application.Current.Resources[StyleKeys.TileBackgroundOrangeKey] as Brush;
        }

        //private Task UpdateItemsAsync()
        //{
        //    var bpProcessManager = IoC.Instance.Resolve<IBPProcessManager>();
        //    return Task.Factory.StartNew(() =>
        //    {
        //        TransportTaskUpdate(bpProcessManager);
        //        //PickingListUpdate();
        //    });
        //}

        ///// <summary>
        ///// Обновление ЗНТ.
        ///// </summary>
        //private void TransportTaskUpdate(IBPProcessManager bpProcessManager)
        //{
        //    int count;
        //    bpProcessManager.GetAvailableTransportTaskCount(null, out count);

        //    TileTransportTaskTitle = count > 0 ? string.Format("{0}", count) : string.Empty;
        //    TileTransportTaskBackground = Application.Current.Resources[count > 0 ? StyleKeys.TileBackgroundOrangeKey : StyleKeys.TileBackgroundDefaultKey] as Brush;
        //}

        ///// <summary>
        ///// Обновление списков пикинга.
        ///// </summary>
        //private async void PickingListUpdate()
        //{
        //    //var count = bpProcessManager.GetPickListCount(null, null);
        //    var count = await _api.GetAvailablePickListCountAsync(WMSEnvironment.Instance.TruckCode);

        //    TilePickingLisTitle = count > 0 ? count.ToString() : string.Empty;
        //    TilePickingListBackground = Application.Current.Resources[
        //        count > 0
        //            ? StyleKeys.TileBackgroundOrangeKey
        //            : StyleKeys.TileBackgroundDefaultKey] as Brush;
        //}
        #endregion .  Methods  .
    }
}
