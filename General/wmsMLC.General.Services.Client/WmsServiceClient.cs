using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.ServiceModel;
using System.Text;
using Cassia;
using log4net;
using Microsoft.Practices.TransientFaultHandling;
using MLC.WebClient;
using wmsMLC.General.Resources;

namespace wmsMLC.General.Services.Client
{
    public class WmsServiceClient : IServiceClient, IDisposable
    {
        #region .  Fields & Consts  .
        private const int DefaultRetryPolicyCount = 2;
        private const int DefaultRetryPolicyTime = 1;

        private readonly ILog _log = LogManager.GetLogger(typeof(WmsServiceClient));
        private readonly IAuthenticationProvider _authenticationProvider;

        private WmsClient _cln;

        private SdclConnectInfo _lastConnectInfo;
        private SdclConnectInfo _nextConnectInfo;

        private int _retryPolicyCount;
        private int _retryPolicyTime;

        private static bool _switchEndpoint;

        private bool? _isConnected;

        private bool _doNotEventHandle;

        private string _sId;

        private volatile Object _clnLock = new object();
        private WmsAPI _api;

        #endregion

        #region .  Properties  .
        /// <summary>
        /// Точка соединения с сервисом
        /// </summary>
        public string EndPoint { get; private set; }

        /// <summary>
        /// Идентификатор сервиса
        /// </summary>
        public string ServiceId { get; private set; }

        /// <summary>
        /// Номер сессии (ProcessId).
        /// </summary>
        public int Session { get; private set; }

        /// <summary>
        /// Id-адрес клиента.
        /// </summary>
        public string IpAddress { get; private set; }

        /// <summary>
        /// Mac-адрес клиента.
        /// </summary>
        public string MacAddress { get; private set; }

        /// <summary>
        /// Имя пользовательского хоста [Host] (DNS имя, IP допустимо) (в том числе и терминальное оборудование)
        /// </summary>
        public string HostName { get; private set; }

        /// <summary>
        /// Уникальное имя клиента
        /// </summary>
        public string ClientName { get; private set; }

        /// <summary>
        /// Тип клиента (ID).
        /// </summary>
        public ClientTypeCode ClientType { get; private set; }

        /// <summary>
        /// Признак наличия соединения
        /// </summary>
        public bool? IsConnected
        {
            get { return _isConnected; }
            private set
            {
                _isConnected = value;
                WMSEnvironment.Instance.IsConnected = _isConnected;
            }
        }

        #endregion

        #region .  ctors  .

        public WmsServiceClient(ClientTypeCode clientType, string serviceEndpoint)
        {
            // по умолчанию sessionId берем по номеру процесса
            // TODO: переделать на уникальный номер (лучше Guid); Как вариант можно завязаться на железные параметры и построить на этом одну из проверок лицензионности
            Session = System.Diagnostics.Process.GetCurrentProcess().Id;

            // вычитываем идентификатор сервиса (по умолчанию DCL)
            // решили пока использовать Guid
            ServiceId = Guid.NewGuid().ToString();
            ClientName = string.Format("{0}_{1}", ServiceId, Session);

            ClientType = clientType;
            EndPoint = serviceEndpoint;

            var manager = new TerminalServicesManager();
            var rdpsession = manager.CurrentSession;
            HostName = rdpsession.ClientName;
            IpAddress = rdpsession.ClientIPAddress.To<string>();
            MacAddress = GetMacAdressByIp(rdpsession.ClientIPAddress);

            if (string.IsNullOrEmpty(HostName)) //HACK: Если не удалось получить данные удаленной сессии, определяем по-умолчанию
                SetDefaultClientSystemProperty();

            _switchEndpoint = true;
            _retryPolicyCount = Properties.Settings.Default.RetryPolicyCount > 0
                ? Properties.Settings.Default.RetryPolicyCount
                : DefaultRetryPolicyCount;

            _retryPolicyTime = Properties.Settings.Default.RetryPolicyTime > 0
                ? Properties.Settings.Default.RetryPolicyTime
                : DefaultRetryPolicyTime;

            IsConnected = false;

            _authenticationProvider = IoC.Instance.Resolve<IAuthenticationProvider>();
            _authenticationProvider.AuthenticatedUserChanging += OnAuthenticatedUserChanging;
            _authenticationProvider.AuthenticatedUserChanged += OnAuthenticatedUserChanged;
        }
        #endregion

        #region .  Methods  .
        private void OnAuthenticatedUserChanging(object s, AuthenticatedUserChangingEventArgs e)
        {
            if (e.NewValue == null) //Logoff
                EndClientSession();
        }

        private void OnAuthenticatedUserChanged(object sender, EventArgs e)
        {
            if (WMSEnvironment.Instance.AuthenticatedUser == null)
                return;
//#if DEBUG
//            // в DEBUG-е авторизация происходит раньше подключения :) а это не везде полезно!
//            GetClient();
//#endif
            StartClientSession();

            //Передаем ид клиентской сессии на сервер, после ее открытия
            //TEST: Тестируем RCL
            if (ClientType == ClientTypeCode.RCL && WMSEnvironment.Instance.SessionId.HasValue)
            {
                try
                {
                    _cln.SetClientSession(WMSEnvironment.Instance.SessionId.Value);
                }
                catch (Exception ex)
                {
                    _log.Error("Error on sending ClientSessionId on server", ex);
                    throw;
                }
            }
        }

        private void SetDefaultClientSystemProperty()
        {
            HostName = Environment.MachineName;
            string ip;
            string mac;
            GetLocalIpAddress(out ip, out mac);
            IpAddress = ip;
            MacAddress = mac;
        }

        #region .  Static helpers  .
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        private static extern int SendARP(int destIp, int srcIp, byte[] pMacAddr, ref uint phyAddrLen);

        private static void GetLocalIpAddress(out string ipaddress, out string macadress)
        {
            ipaddress = null;
            macadress = null;
            var host = Dns.GetHostEntry(Dns.GetHostName());
            if (host == null || host.AddressList == null || host.AddressList.Length == 0) return;
            foreach (var p in host.AddressList)
            {
                if (p.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipaddress = p.ToString();
                    macadress = GetMacAdressByIp(p);
                    return;
                }
            }
        }

        private static string GetMacAdressByIp(IPAddress ipAddress)
        {
            if (ipAddress == null)
                return null;
            var macAddr = new byte[6];
            var macAddrLen = (uint)macAddr.Length;
            try
            {
                if (SendARP(BitConverter.ToInt32(ipAddress.GetAddressBytes(), 0), 0, macAddr, ref macAddrLen) != 0)
                    return null;
            }
            catch //нельзя получить
            {
                return null;
            }
            var macAddressString = new StringBuilder();
            foreach (var t in macAddr)
            {
                if (macAddressString.Length > 0)
                    macAddressString.Append(":");
                macAddressString.AppendFormat("{0:X2}", t);
            }
            return macAddressString.ToString();
        }
        #endregion

        private void StartClientSession()
        {
            //Открываем сессию
            try
            {
                var sessionRegistrator = IoC.Instance.Resolve<ISessionRegistrator>();
                sessionRegistrator.StartNewSession(HostName, ClientType, HostName, IpAddress, MacAddress,
                    Session.ToString(CultureInfo.InvariantCulture),
                    string.Concat(Environment.UserDomainName, "\\", Environment.UserName),
                    AssemblyAttributeAccessors.GetAssemblyFileVersion(Assembly.GetEntryAssembly()),
                    WMSEnvironment.Instance.SdclCode,
                    Environment.OSVersion.VersionString);
            }
            catch (Exception ex)
            {
                CorrectLogOff();

                _log.Error("Ошибка открытия сессии", ex);
                throw;
            }
        }

        private void EndClientSession()
        {
            // не заходим сюда снова при LogOff
            if (_doNotEventHandle || !WMSEnvironment.Instance.SessionId.HasValue)
                return;

            //Закрываем сессию
            try
            {
                var sessionRegistrator = IoC.Instance.Resolve<ISessionRegistrator>();
                sessionRegistrator.EndCurrentSession(true); //Корректное закрытие сессии
            }
            catch (Exception ex)
            {
                CorrectLogOff();

                _log.Error(ex);
                throw;
            }
            finally
            {
                Disconnect();
            }
        }

        private void CorrectLogOff()
        {
            if (_authenticationProvider == null)
                return;

            _doNotEventHandle = true;
            try
            {
                _authenticationProvider.LogOff();
            }
            finally
            {
                _doNotEventHandle = false;
            }
        }

        private EndpointAddress GetEndpoint()
        {
            if (!_switchEndpoint)
            {
                if (!string.IsNullOrEmpty(WMSEnvironment.Instance.EndPoint))
                    return new EndpointAddress(WMSEnvironment.Instance.EndPoint);
            }

            EndpointAddress endPoint;

            // если парметры выставили извне ("жетская привязка"), то используем что передали
            if (!string.IsNullOrEmpty(EndPoint))
            {
                endPoint = new EndpointAddress(EndPoint);
                WMSEnvironment.Instance.SdclCode = "Default";
            }
            else
            {
                // если дошли до конца списка доступных сервисов, то пойдем с начала
                if (_lastConnectInfo != null && string.IsNullOrEmpty(_lastConnectInfo.Endpoint))
                    _lastConnectInfo.Code = null;

                if (_nextConnectInfo != null)
                    _lastConnectInfo = _nextConnectInfo;
                else
                {
                    // получим параметры подключения
                    //var sdclConnectInfoProvider = IoC.Instance.Resolve<ISdclConnectInfoProvider>();
                    //_lastConnectInfo = sdclConnectInfoProvider.GetSdclConnectInfo(HostName, _lastConnectInfo);

                    // обращаемся
                    _lastConnectInfo = GetSdclConnectInfo(HostName, _lastConnectInfo);
                }

                if (_lastConnectInfo == null || string.IsNullOrEmpty(_lastConnectInfo.Endpoint))
                    throw new EndpointNotFoundException("Не определен адрес подключения.\r\nЕсли повторные попытки вызывают ту же ошибку, пожалуйста, обратитесь в службу поддержки");

                endPoint = new EndpointAddress(_lastConnectInfo.Endpoint);
                WMSEnvironment.Instance.SdclCode = _lastConnectInfo.Code ?? "Default";
            }

            _nextConnectInfo = null;
            WMSEnvironment.Instance.EndPoint = endPoint.ToString();
            _switchEndpoint = false;
            return endPoint;
        }

        private SdclConnectInfo GetSdclConnectInfo(string clientCode, SdclConnectInfo lastSdclConnectInfo)
        {
            if (_api == null)
                _api = IoC.Instance.Resolve<WmsAPI>();

            var result  = _api.GetSdclEndPoint(clientCode, lastSdclConnectInfo == null ? null : lastSdclConnectInfo.Code);
            return result;
        }

        private void Disconnect()
        {
            try
            {
                if (_cln == null)
                    return;

                if (_cln.State == CommunicationState.Opened)
                {
                    var sessionId = _cln.TerminateSession();
                    if (_sId.EqIgnoreCase(sessionId))
                        _log.DebugFormat("Terminate session id '{0}'", sessionId);
                    else
                        _log.DebugFormat("Terminate session id '{0}' but expected '{1}'", sessionId, _sId);
                }
            }
            catch (Exception ex)
            {
                _log.Debug(ex);
            }
            finally
            {
                if (_cln != null)
                {
                    _cln.Abort();
                    _cln.ChannelFactory.Closed -= ChannelFactoryOnClosed;
                    _cln.ChannelFactory.Faulted -= ChannelFactoryOnFaulted;
                    _cln.ChannelFactory.Opened -= ChannelFactoryOnOpened;
                    _cln.InnerChannel.Faulted -= InnerChannelOnFaulted;
                    _cln.InnerChannel.Closed -= InnerChannelOnClosed;
                    _cln.InnerChannel.Opened -= InnerChannelOnOpened;
                    _cln.InnerChannel.UnknownMessageReceived -= InnerChannel_UnknownMessageReceived;
                }
                _cln = null;
            }
        }

        public Telegram Process(TelegramBodyType type, Telegram telegram)
        {
            try
            {
                var policy = new RetryPolicy(new TelegramProcessingErrorDetectionStrategy(telegram),
                    new FixedInterval(_retryPolicyCount, TimeSpan.FromSeconds(_retryPolicyTime)));

                return policy.ExecuteAction(() => ProcessTelegram(telegram));
            }
            catch (Exception ex)
            {
                //                if (ex is UnauthorizedAccessException)
                //                    throw;
                if (ex is TimeoutException)
                {
                    _log.Error(ex);
                }
                else
                {
                    IsConnected = false;
                    _log.Error("Ошибка отправки телеграммы", ex);
                }
                throw;
            }
        }

        internal class TelegramProcessingErrorDetectionStrategy : ITransientErrorDetectionStrategy
        {
            private readonly bool _isLongUnitOfWork;

            public TelegramProcessingErrorDetectionStrategy() { }

            public TelegramProcessingErrorDetectionStrategy(Telegram telegram)
            {
                _isLongUnitOfWork = !telegram.UnitOfWork.Equals(Guid.Empty);
            }
            public bool IsTransient(Exception ex)
            {
                var transient = ex is EndpointNotFoundException || ex is TimeoutException || ex is FaultException ||
                                ex is CommunicationException;
                // переключаем сервис, если не находимся в транзакции
                if (!_isLongUnitOfWork && transient)
                    _switchEndpoint = true;
                //INFO: InvalidOperationException возникает, если канал еще не открыт, а телеграмму уже отправляем
                //INFO: этого уже не должно происходить, но оставил на всякий
                return _isLongUnitOfWork || transient || ex is InvalidOperationException;
            }
        }

        protected static int DefaultTimeout = 30000;

        private Telegram ProcessTelegram(Telegram telegram)
        {
            if (telegram == null)
                throw new ArgumentNullException("telegram");

            // заполняем служебные поля телеграммы
            FillSysFields(telegram);

            // сериализуем телеграмму в бинарный формат
            var tmp = GSerialize.SerializeBytes(telegram);

            var client = GetClient();
            var resultData = client.ProcessTelegramAsync(tmp);
            var waitTime = telegram.TimeOut ?? DefaultTimeout;
            if (!resultData.Wait(waitTime == 0 ? -1 : waitTime))
                throw new TimeoutException(ExceptionResources.TimeoutExceptionMessage);

            var data = resultData.Result;
            if (data == null)
                return null;

            var resultTelegram = GSerialize.DeserializeBytes<Telegram>(data);
            return resultTelegram;
        }

        private WmsClient GetClient()
        {
            // если клиент уже открыть, то вернем сразу
            if (!_switchEndpoint && _cln != null && _cln.State == CommunicationState.Opened)
                return _cln;

            lock (_clnLock)
            {
                var lastSdcl = WMSEnvironment.Instance.SdclCode;
                if (_switchEndpoint || (_cln == null || _cln.State == CommunicationState.Faulted || _cln.State == CommunicationState.Closed ||
                    _cln.State == CommunicationState.Closing))
                {
                    Disconnect();
                    var binding = new NetTcpBinding("NetTcpBinding");
                    var endPoint = GetEndpoint();
                    _cln = new WmsClient(binding, endPoint);
                    //_cln.InnerChannel.OperationTimeout = new TimeSpan(0, 10, 0);
                    _cln.ChannelFactory.Closed += ChannelFactoryOnClosed;
                    _cln.ChannelFactory.Faulted += ChannelFactoryOnFaulted;
                    _cln.ChannelFactory.Opened += ChannelFactoryOnOpened;
                    _cln.InnerChannel.Faulted += InnerChannelOnFaulted;
                    _cln.InnerChannel.Closed += InnerChannelOnClosed;
                    _cln.InnerChannel.Opened += InnerChannelOnOpened;
                    _cln.InnerChannel.UnknownMessageReceived += InnerChannel_UnknownMessageReceived;
                    _log.DebugFormat("Create new communication client");
                }

                // пока ждали лок, могли уже открыть сессию
                if (_cln.State == CommunicationState.Opened)
                    return _cln;

                // стратуем сессию
                _sId = _cln.StartSession(ClientType, WMSEnvironment.Instance.SessionId);

                _log.DebugFormat("Session '{0}' started", _sId);

                // обновим сессию, если был разрыв канала
                if (WMSEnvironment.Instance.SessionId.HasValue && WMSEnvironment.Instance.SdclCode != lastSdcl)
                {
                    var clientProvider = IoC.Instance.Resolve<ISessionRegistrator>();
                    clientProvider.UpdateCurrentSession(WMSEnvironment.Instance.SdclCode);
                }
                return _cln;
            }
        }

        void InnerChannel_UnknownMessageReceived(object sender, UnknownMessageReceivedEventArgs e)
        {
            _log.Debug(e.Message);
        }

        private void InnerChannelOnFaulted(object sender, EventArgs e)
        {
            IsConnected = null;

            _log.Debug("Service communication channel is faulted");

            if (_cln != null)
                _cln.Abort();
        }

        private void InnerChannelOnOpened(object sender, EventArgs eventArgs)
        {
            IsConnected = true;
        }

        private void InnerChannelOnClosed(object sender, EventArgs eventArgs)
        {
            IsConnected = null;
        }

        private void ChannelFactoryOnOpened(object sender, EventArgs eventArgs)
        {
        }

        private void ChannelFactoryOnFaulted(object sender, EventArgs eventArgs)
        {
            IsConnected = false;

            _log.Debug("Client communication channel is faulted");

            if (_cln != null)
                _cln.Abort();
        }

        private void ChannelFactoryOnClosed(object sender, EventArgs eventArgs)
        {
        }

        private void FillSysFields(Telegram telegram)
        {
            // проставляем подпись
            SubscribeTelegram(telegram);

            // параметры отправки
            telegram.FromId.ServiceId = ServiceId;
            telegram.FromId.SessionId = Session;
            telegram.FromId.ClientSessionId = WMSEnvironment.Instance.SessionId;
        }

        /// <summary>
        /// Удаление объекта клиента
        /// </summary>
        public void Dispose()
        {
            _log.Debug("Client was disposed");
            if (_cln == null)
                return;

            try
            {
                Disconnect();
            }
            catch (Exception ex)
            {
                _log.Error("Ошибка при выгрузке клиента", ex);
            }
        }

        /// <summary> Подписываем телеграмму.</summary>
        /// <param name="telegram">Телеграмма, которую необходимо подписать</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="System.Security.Authentication.AuthenticationException">Взводится при 1) не доступен провайдер авторизации; 2) пользователь не автоизован; 3) нет возможности получить подпись пользователя</exception>
        private void SubscribeTelegram(Telegram telegram)
        {
            if (telegram == null)
                throw new ArgumentNullException("telegram");

            //HACK если мы в процессе авторизации - ничего подписывать не нужно
            if (_authenticationProvider.InAuthentication && WMSEnvironment.Instance.AuthenticatedUser == null)
                return;

            // если у нас ответ на пинг, то подписываем в любом случае
            var pingAck = telegram.Type == TelegramType.Answer &&
                          telegram.Content != null &&
                          telegram.Content.Name == "CMD" &&
                          telegram.Content.Value == "ACK";

            // никто не залогинен
            if (!pingAck && WMSEnvironment.Instance.AuthenticatedUser == null)
                throw new AuthenticationException(ExceptionResources.UserNotAuthenticated);

            // нет подписи
            var userId = WMSEnvironment.Instance.AuthenticatedUser == null ? null : WMSEnvironment.Instance.AuthenticatedUser.GetSignature();
            if (!pingAck && string.IsNullOrEmpty(userId))
                throw new AuthenticationException(ExceptionResources.UserSignatureNotValid);

            // подписываем
            telegram.UserInfo = userId;
        }

        public void Reconnect(SdclConnectInfo info)
        {
            _nextConnectInfo = info;
            Disconnect();
            _switchEndpoint = true;
            //GetClient();
            Process(TelegramBodyType.Wms, new Telegram(TelegramType.QueryNoAnswer));
        }
        #endregion
    }
}