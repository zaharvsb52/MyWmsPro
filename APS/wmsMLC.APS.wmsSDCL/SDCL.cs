using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using wmsMLC.General;
using wmsMLC.General.Services;
using wmsMLC.General.Services.Service;

namespace wmsMLC.APS.wmsSDCL
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        IncludeExceptionDetailInFaults = true,
        AddressFilterMode = AddressFilterMode.Any,
        AutomaticSessionShutdown = true,
        Namespace = "http://wms.my.ru/services/")]
    [AspNetCompatibilityRequirements(
     RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class SDCL : IWmsService
    {
        /// <summary>
        /// Идентификатор службы.
        /// </summary>
        [Obsolete("Стремимся уйти от серверных зависимостей. Ни какой специфики между сервисами")]
        public static string HandlerId;

        public static int SessionCount { get; private set; }

        #region .  Fields & Constans .
        private const int DefaultChannelTimeout = 60000;

        protected readonly ILog Log;
        private readonly IServiceManager _manager;
        private string _sessionId;
        private readonly Timer _channelTimer;
        private volatile int _processes;
        private int _channelTimeout;
        private IContextChannel _channel;
        private readonly CancellationTokenSource _cancelOperation;
        private ClientTypeCode _clienttype;
        private decimal? _clientSessionId;
        private bool _isClientSessionIdNull;
        #endregion .  Fields & Constans .

        public SDCL()
        {
            Log = LogManager.GetLogger(GetType());
            _manager = IoC.Instance.Resolve<WmsServiceManager>();

            _cancelOperation = new CancellationTokenSource();
            _channelTimer = new Timer(AbortChannel);
        }

        private void AbortChannel(object state)
        {
            if (_channel != null && _processes < 1)
            {
                Log.DebugFormat("Channel for session {0} with client session {1} will be aborted by timeout {2}.", _sessionId, _clientSessionId, _channelTimeout);
                //TEST: Тестируем RCL
                if (_clienttype == ClientTypeCode.RCL)
                {
                    if (_clientSessionId.HasValue)
                    {
                        try
                        {
                            var sessionRegistrator = IoC.Instance.Resolve<ISessionRegistrator>();
                            sessionRegistrator.EndSession(_clientSessionId.Value, false); //Некорректное закрытие сессии
                        }
                        catch (Exception ex)
                        {
                            Log.Error(string.Format("Error on close ClientSession ('{0}')", _clientSessionId), ex);   
                        }
                        _clientSessionId = null;
                        _isClientSessionIdNull = true;
                    }
                    else
                    {
                        if (!_isClientSessionIdNull)
                            Log.Error(new DeveloperException("_clientSessionId is undefined."));
                    }
                }
                _channel.Abort();
            }
        }

        public virtual string StartSession(ClientTypeCode clientType, decimal? clientSessionId)
        {
            SessionCount++;

            _sessionId = OperationContext.Current.SessionId;
            _channel = OperationContext.Current.Channel;

            _clienttype = clientType;
            _clientSessionId = clientSessionId;
            _isClientSessionIdNull = false;
            //TEST: Тестируем RCL
            if (_clienttype == ClientTypeCode.RCL && _clientSessionId.HasValue)
            {
                try
                {
                    var sessionRegistrator = IoC.Instance.Resolve<ISessionRegistrator>();
                    sessionRegistrator.ReopenSession(_clientSessionId.Value);
                }
                catch(Exception ex)
                {
                    Log.Error(string.Format("Error on reopen ClientSession ('{0}')", _clientSessionId), ex);
                }
            }

            _channel.Closed += Channel_Closed;

            //INFO: пока отключили контроль количества клиентов
            //if (SessionCount > Properties.Settings.Default.MaxConnections)
            //{
            //    SessionCount--;
            //    Log.DebugFormat("Client session '{0}' rejected. Max session count is {1}", _sessionId, Properties.Settings.Default.MaxConnections);
            //    throw new FaultException("Превышено максимальное количество сессий");
            //}
            _channelTimeout = Properties.Settings.Default.ChannelTimeout > 0
                ? Properties.Settings.Default.ChannelTimeout
                : DefaultChannelTimeout;

            Log.DebugFormat("Client session '{0}' started. Total sessions count {1}", _sessionId, SessionCount);
            _channelTimer.Change(_channelTimeout, _channelTimeout);
            return _sessionId;
        }

        public void SetClientSession(decimal clientSessionId)
        {
            _clientSessionId = clientSessionId;
            _isClientSessionIdNull = false;
        }

        public virtual string TerminateSession()
        {
            Log.DebugFormat("Channel for session {0} with client session {1} was terminated by user request.", _sessionId, _clientSessionId);
            ReleaseResources();

            return _sessionId;
        }

        private async Task<byte[]> ProcessTelegramTaskBasedAsync(byte[] data)
        {
            try
            {
                _processes++;
                if (_processes > 1)
                    Log.DebugFormat("Parallel processing of {0} telegrams", _processes);

                _channelTimer.Change(_channelTimeout, _channelTimeout);
                if (_manager == null)
                    throw new OperationException("IServiceManager is not initialized");

                return await Task.Factory.StartNew(() =>
                {
                    if (data == null || data.Length == 0)
                    {
                        Log.Warn("Receive clear data (null or length=0");
                        return null;
                    }

                    //жестко обрубаем поток, в случае отмены операции
                    using (_cancelOperation.Token.Register(Thread.CurrentThread.Abort))
                    {
                        // разжимаем в телеграмму
                        var telegram = GSerialize.DeserializeBytes<Telegram>(data);

                        // обрабатываем
                        var resultTelegram = _manager.ProcessTelegram(telegram);

                        // сериализуем ответ
                        return GSerialize.SerializeBytes(resultTelegram);
                    }
                }, _cancelOperation.Token);
            }
            catch (Exception ex)
            {
                if (_cancelOperation.IsCancellationRequested)
                    Log.DebugFormat("All processes were cancelled for client session '{0}'", _sessionId);
                else
                    Log.Debug(ex);
                return null;
            }
            finally
            {
                _processes--;
            }
        }

        public byte[] ProcessTelegram(byte[] data)
        {
            return ProcessTelegramTaskBasedAsync(data).Result;
        }

        public IAsyncResult BeginProcessTelegram(byte[] data, AsyncCallback callback, object asyncState)
        {
            //var data = asyncState as byte[];
            var task = ProcessTelegramTaskBasedAsync(data);
            if (callback != null)
                task.ContinueWith(_ => callback(task));

            return task;
        }

        public byte[] EndProcessTelegram(IAsyncResult result)
        {
            return ((Task<byte[]>)result).Result;
        }

        private void Channel_Closed(object sender, EventArgs e)
        {
            SessionCount--;
            ReleaseResources();
            Log.DebugFormat("Client session '{0}' terminated. Remaining {1} sessions", _sessionId, SessionCount);
        }

        private void ReleaseResources()
        {
            _channelTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _channelTimer.Dispose();

            _cancelOperation.Cancel(false);
            if (_manager != null)
                _manager.Close();

            if (_channel != null)
                _channel.Closed -= Channel_Closed;
        }
    }
}
