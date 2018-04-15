using System;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Media;
using MLC.WebClient;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Components.Helpers;

namespace wmsMLC.RCL.Main.ViewModels
{
    public class TTaskMenuViewModel : MenuTileViewModelBase
    {
        #region .  Fields & Consts  .

        public const string ActionTTaskAll = "TransportTaskAll";
        public const string ActionTTaskMove = "TransportTaskMove";
        public const string ActionTTaskOutput = "TransportTaskOutput";
        public const string ActionTTaskIn = "TransportTaskIn";

        public const string ActionRefresh = "Refresh";

        private const string FilterMove = "TTASKTYPECODE_R = 'MOVE' ";
        private const string FilterIn = "TTASKTYPECODE_R = 'IN' ";
        private const string FilterOutput = "TTASKTYPECODE_R = 'OUT' ";

        private readonly WmsAPI _api;

        #endregion

        public TTaskMenuViewModel()
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

        private string _tileTTaskAllHeader;

        public string TileTTaskAllHeader
        {
            get { return _tileTTaskAllHeader; }
            set
            {
                if (_tileTTaskAllHeader == value)
                    return;
                _tileTTaskAllHeader = value;
                OnPropertyChanged("TileTTaskAllHeader");
            }
        }

        private string _tileTTaskOutputHeader;

        public string TileTTaskOutputHeader
        {
            get { return _tileTTaskOutputHeader; }
            set
            {
                if (_tileTTaskOutputHeader == value)
                    return;
                _tileTTaskOutputHeader = value;
                OnPropertyChanged("TileTTaskOutputHeader");
            }
        }

        private string _tileTTaskInHeader;

        public string TileTTaskInHeader
        {
            get { return _tileTTaskInHeader; }
            set
            {
                if (_tileTTaskInHeader == value)
                    return;
                _tileTTaskInHeader = value;
                OnPropertyChanged("TileTTaskInHeader");
            }
        }

        private string _tileTTaskMoveHeader;

        public string TileTTaskMoveHeader
        {
            get { return _tileTTaskMoveHeader; }
            set
            {
                if (_tileTTaskMoveHeader == value)
                    return;
                _tileTTaskMoveHeader = value;
                OnPropertyChanged("_tileTTaskMoveHeader");
            }
        }

        private string _tileTTaskMoveTitle;

        public string TileTTaskMoveTitle
        {
            get { return _tileTTaskMoveTitle; }
            set
            {
                if (_tileTTaskMoveTitle == value)
                    return;
                _tileTTaskMoveTitle = value;
                OnPropertyChanged("TileTTaskMoveTitle");
            }
        }

        private string _tileTTaskInTitle;

        public string TileTTaskInTitle
        {
            get { return _tileTTaskInTitle; }
            set
            {
                if (_tileTTaskInTitle == value)
                    return;
                _tileTTaskInTitle = value;
                OnPropertyChanged("TileTTaskInTitle");
            }
        }

        private string _tileTTaskOutputTitle;

        public string TileTTaskOutputTitle
        {
            get { return _tileTTaskOutputTitle; }
            set
            {
                if (_tileTTaskOutputTitle == value)
                    return;
                _tileTTaskOutputTitle = value;
                OnPropertyChanged("TileTTaskOutputTitle");
            }
        }

        private string _tileTTaskAllTitle;

        public string TileTTaskAllTitle
        {
            get { return _tileTTaskAllTitle; }
            set
            {
                if (_tileTTaskAllTitle == value)
                    return;
                _tileTTaskAllTitle = value;
                OnPropertyChanged("TileTTaskAllTitle");
            }
        }

        private Brush _tileTTaskAllBackground;

        public Brush TileTTaskAllBackground
        {
            get { return _tileTTaskAllBackground; }
            set
            {
                if (Equals(_tileTTaskAllBackground, value))
                    return;
                _tileTTaskAllBackground = value;
                OnPropertyChanged("TileTTaskAllBackground");
            }
        }

        private Brush _tileTTaskOutputBackground;

        public Brush TileTTaskOutputBackground
        {
            get { return _tileTTaskOutputBackground; }
            set
            {
                if (Equals(_tileTTaskOutputBackground, value))
                    return;
                _tileTTaskOutputBackground = value;
                OnPropertyChanged("TileTTaskOutputBackground");
            }
        }

        private Brush _tileTTaskInBackground;

        public Brush TileTTaskInBackground
        {
            get { return _tileTTaskInBackground; }
            set
            {
                if (Equals(_tileTTaskInBackground, value))
                    return;
                _tileTTaskInBackground = value;
                OnPropertyChanged("TileTTaskInBackground");
            }
        }

        private Brush _tileTTaskMoveBackground;

        public Brush TileTTaskMoveBackground
        {
            get { return _tileTTaskMoveBackground; }
            set
            {
                if (Equals(_tileTTaskMoveBackground, value))
                    return;
                _tileTTaskMoveBackground = value;
                OnPropertyChanged("TileTTaskMoveBackground");
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

        protected override Action DefaultCompletedActionHandler
        {
            get { return UpdateItems; }
        }

        #endregion .  Properties  .

        #region .  Methods  .

        #region .  Create Menu  .

        public override void InitializeMenu()
        {
            TileTTaskInHeader = Resources.StringResources.In;
            TileTTaskInTitle = string.Empty;

            TileTTaskOutputHeader = Resources.StringResources.Out;
            TileTTaskOutputTitle = string.Empty;

            TileTTaskMoveHeader = Resources.StringResources.Move;
            TileTTaskMoveTitle = string.Empty;

            TileTTaskAllHeader = Resources.StringResources.AllTask;
            TileTTaskAllTitle = string.Empty;

            TileTTaskAllBackground = Application.Current.Resources[StyleKeys.TileBackgroundDefaultKey] as Brush;
            TileTTaskMoveBackground = Application.Current.Resources[StyleKeys.TileBackgroundDefaultKey] as Brush;
            TileTTaskInBackground = Application.Current.Resources[StyleKeys.TileBackgroundDefaultKey] as Brush;
            TileTTaskOutputBackground = Application.Current.Resources[StyleKeys.TileBackgroundDefaultKey] as Brush;

            RefreshTileDefault();
        }

        #endregion .  Create Menu  .

        #region . Commands .

        protected override void OnMenuClick(string parameter)
        {
            if (!CanMenuClick(parameter))
                return;

            switch (parameter)
            {
                case ActionTTaskAll:
                    ShowTransportTaskMenu(string.Empty);
                    break;
                case ActionTTaskMove:
                    ShowTransportTaskMenu(FilterMove);
                    break;
                case ActionTTaskOutput:
                    ShowTransportTaskMenu(FilterOutput);
                    break;
                case ActionTTaskIn:
                    ShowTransportTaskMenu(FilterIn);
                    break;
                case ActionRefresh:
                    UpdateItems();
                    break;
            }
        }

        #endregion . Commands .

        private async void ShowTransportTaskMenu(string filter)
        {
            try
            {
                WaitIndicatorVisible = true;
                var context = new BpContext();
                context.Properties.Add("MenuCommandId", 0);
                context.Properties.Add("Restart", true);
                context.Properties.Add("Filter", filter);
                await RunBpAsync("RclTransportTaskNotification", context, DefaultCompletedHandler);
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

                if (!WMSEnvironment.Instance.SessionId.HasValue)
                    throw new Exception("SessionId is null");

                var transportTaskByTypes = await _api.GetAvailableTransportTaskCountAsync(Convert.ToInt32(WMSEnvironment.Instance.SessionId.Value)).ConfigureAwait(false);

                var inCount = transportTaskByTypes.ContainsKey("IN") ? transportTaskByTypes["IN"] : 0;
                var moveCount = transportTaskByTypes.ContainsKey("MOVE") ? transportTaskByTypes["MOVE"] : 0;
                var outCount = transportTaskByTypes.ContainsKey("OUT") ? transportTaskByTypes["OUT"] : 0;
                var totalCount = transportTaskByTypes.Values.Sum();

                TileTTaskInTitle = inCount > 0 ? inCount.ToString() : string.Empty;
                TileTTaskInBackground =
                    Application.Current.Resources[
                        inCount > 0 ? StyleKeys.TileBackgroundOrangeKey : StyleKeys.TileBackgroundDefaultKey] as Brush;
                TileTTaskOutputTitle = outCount > 0 ? outCount.ToString() : string.Empty;
                TileTTaskOutputBackground =
                    Application.Current.Resources[
                        outCount > 0 ? StyleKeys.TileBackgroundOrangeKey : StyleKeys.TileBackgroundDefaultKey] as Brush;
                TileTTaskMoveTitle = moveCount > 0 ? moveCount.ToString() : string.Empty;
                TileTTaskMoveBackground =
                    Application.Current.Resources[
                        moveCount > 0 ? StyleKeys.TileBackgroundOrangeKey : StyleKeys.TileBackgroundDefaultKey] as Brush;
                TileTTaskAllTitle = totalCount > 0 ? totalCount.ToString() : string.Empty;
                TileTTaskAllBackground =
                    Application.Current.Resources[
                        totalCount > 0 ? StyleKeys.TileBackgroundOrangeKey : StyleKeys.TileBackgroundDefaultKey] as
                        Brush;
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

        #endregion .  Methods  .
    }
}