//using System;
//using System.Linq;
//using System.Net;
//using System.Net.Sockets;
//using System.ServiceModel;
//using System.Threading.Tasks;
//using log4net;
//using wmsMLC.General.Resources;
//using wmsMLC.General.Types;

//namespace wmsMLC.General.Service.Client
//{

//    /// <summary>
//    /// Класс сервисов-клиентов
//    /// </summary>
//    [Obsolete("Пробовали перейти на WinSock - пока безрезультатно. Данный клас используется только в тестах")]
//    public class SocketServiceClient : BaseObject, IServiceClient, IDisposable
//    {
//        private readonly ILog _log = LogManager.GetLogger(typeof(SocketServiceClient));

//        public static TimeSpan DefaultReceiveTimeout = new TimeSpan(0, 3, 0);
//        public static TimeSpan DefaultSendTimeout = new TimeSpan(0, 3, 0);

//        private ConnectionInfo _connection;

//        /// <summary>
//        /// Точка соединения с сервисом.
//        /// </summary>
//        public string EndPoint { get; set; }

//        /// <summary>
//        /// Признак наличия соединения
//        /// </summary>
//        private volatile bool _isConnected;

//        private IPreProcessor _preProcessor;

//        public new event EventErrorHandler OnEventErrorHandler;

//        /// <summary>
//        /// Идентификатор сервиса.
//        /// </summary>
//        public string ServiceId { get; set; }

//        /// <summary>
//        /// Номер сессии.
//        /// </summary>
//        public int Session { get; set; }

//        public decimal? _clientSessionId { get; private set; }

//        /// <summary>
//        /// Id-адрес клиента.
//        /// </summary>
//        public string IpAddress { get; set; }

//        /// <summary>
//        /// Mac-адрес клиента.
//        /// </summary>
//        public string MacAddress { get; set; }

//        /// <summary>
//        /// Тип клиента (ID).
//        /// </summary>
//        public string ClientType { get; set; }

//        public string UserId { get; private set; }

//        public event ReceiveTelegram OnReceiveTelegram;

//        /// <summary>
//        /// Конструктор класса.
//        /// </summary>
//        public SocketServiceClient() : this(string.Empty) { }

//        /// <summary>
//        /// Конструктор класса.
//        /// </summary>
//        /// <param name="linkById">привязка по Id</param>
//        public SocketServiceClient(string linkById)
//        {
//            LinkById = linkById;
//            _preProcessor = new TelegramPreProcessor(LinkById);
//            _preProcessor.Ack += _preProcessor_Ack;
//        }

//        /// <summary>
//        /// Ответ на ping
//        /// </summary>
//        /// <param name="type">тип комманды</param>
//        /// <param name="telegram">телеграмма пинга</param>
//        void _preProcessor_Ack(TelegramBodyType type, Telegram telegram)
//        {
//            ToId tmp = telegram.ToId;
//            telegram.ToId = telegram.FromId;
//            telegram.Type = TelegramType.Answer;
//            telegram.FromId = tmp;
//            OnTelegramOut(TelegramBodyType.Wms, telegram);
//        }

//        /// <summary>
//        /// Соединение с службой.
//        /// </summary>
//        /// <returns>true в случае успеха</returns>
//        private void Connect()
//        {
//            throw new NotImplementedException("Клиент не поддерживается");
///*
//            try
//            {
//                _connection = new ConnectionInfo
//                    {
//                        Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
//                    };

//                var ss = EndPoint.Split(':');
//                var port = ss[1].To(-1);
//                var add = IPAddress.Parse(ss[0]);
//                var clientEndpoint = new IPEndPoint(add, port);
//                _connection.Socket.Connect(clientEndpoint);
//                _connection.Socket.BeginReceive(_connection.Buffer, 0, _connection.Buffer.Length, SocketFlags.None, ReceiveCallback, _connection);

//                //_client.Connect(LinkById);
//                var bytes = new SocketMessageHelper().SetMessage(SocketMessageTemplate.ActionType.Connect, LinkById);
//                _connection.Socket.Send(bytes, bytes.Length, SocketFlags.None);
//                _isConnected = true;
//            }
//            catch (Exception ex)
//            {
//                if (OnEventErrorHandler != null)
//                    OnEventErrorHandler(ex, ErrorType.ERROR, "EConnect", new string[] { ServiceId }, "SERVICE_CLIENT_CONNECT", LinkById);

//                _isConnected = false;
//                throw;
//            }
//*/
//        }

//        private void ReceiveCallback(IAsyncResult result)
//        {
//            var info = (ConnectionInfo) result.AsyncState;
//            try
//            {
//                int bytestoread = info.Socket.EndReceive(result);
//                if (bytestoread > 0)
//                {
//                    //string text = Encoding.UTF8.GetString(info.buffer, 0, bytestoread);
//                    //Console.Write(text);
//                    var message = new SocketMessageHelper().GetMessage(info.Buffer);
//                    if (message.Action.To(SocketMessageTemplate.ActionType.None) != SocketMessageTemplate.ActionType.Answer)
//                        throw new DeveloperException("Bad answer.");
//                    ReceiveCallback(message.Result);
//                    info.Socket.BeginReceive(info.Buffer, 0, info.Buffer.Length, SocketFlags.None, ReceiveCallback, info);
//                }
//                else
//                {
//                    info.Socket.Close();
//                }
//            }
//            catch (Exception ex)
//            {
//                //if (OnEventErrorHandler != null)
//                //    OnEventErrorHandler(ex, ErrorType.ERROR, "ReceiveCallback", new string[] { ServiceId }, "SERVICE_CLIENT_SEND", LinkById);
//                throw;
//            }
//        }

//        public Telegram Process(TelegramBodyType type, Telegram telegram)
//        {
//            throw new NotImplementedException();
//        }

//        /// <summary>
//        /// Отправка телеграммы адресату
//        /// </summary>
//        /// <param name="telegram">объект телеграммы</param>
//        public bool SendTelegram(byte[] telegram)
//        {
//            //            lock (_clientLocker)
//            //            {
//            // если не было подключения - подключаемся
//            try
//            {
//                SendTelegramWithReTry(telegram, true);
//                return true;
//            }
//            catch (Exception ex)
//            {
//                _isConnected = false;

//                if (OnEventErrorHandler != null)
//                    OnEventErrorHandler(ex, ErrorType.ERROR, "ESendService", new[] {ServiceId}, "SERVICE_CLIENT_SEND", LinkById);
//            }
//            return false;
//        }

//        private void SendTelegramWithReTry(byte[] telegram, bool needMoreTries)
//        {
//            try
//            {
//                if (!_isConnected) Connect();

//                //_client.SendTelegram(telegram);
//                var bytes = new SocketMessageHelper().SetMessage(SocketMessageTemplate.ActionType.SendTelegram, telegram);
//                _connection.Socket.Send(bytes, bytes.Length, SocketFlags.None);
//            }
//            catch (CommunicationObjectFaultedException ex)
//            {
//                _isConnected = false;

//                if (OnEventErrorHandler != null)
//                    OnEventErrorHandler(ex, ErrorType.ERROR, "ESendService", new string[] { ServiceId }, "SERVICE_CLIENT_SEND", LinkById);

//                if (needMoreTries)
//                    SendTelegramWithReTry(telegram, false);
//            }
//        }

//        /// <summary>
//        /// Обработка отправляемой телеграммы
//        /// </summary>
//        /// <param name="type">тип телеграммы</param>
//        /// <param name="telegram">объект телеграммы</param>
//        public virtual void OnTelegramOut(TelegramBodyType type, Telegram telegram)
//        {
//            if (telegram != null)
//            {
//                telegram.FromId.ServiceId = ServiceId;
//                telegram.FromId.SessionId = Session;
//                // сериализуем телеграмму в бинарный формат
//                var tmp = GSerialize.SerializeBytes(telegram);
//                // создадим новый массив с первым байтом типа телеграммы
//                var data = new byte[tmp.Length + 1];
//                // укажем тип телеграммы
//                data[0] = (byte)type;
//                // копируем телеграмму в новый массив с первым байтом
//                Array.Copy(tmp, 0, data, 1, tmp.Length);
//                // ПОСЫЛАЕМ ТЕЛЕГРАММУ
//                if (this.SendTelegram(data))
//                {
//                    if (OnEventErrorHandler != null)
//                        OnEventErrorHandler(null, ErrorType.INFO, "TelegramOut", new string[] { telegram.ToId.ToString() }, "SERVICE_CLIENT_TELEGRAMOUT", LinkById);
//                }
//                else
//                {
//                    var ex = new CommunicationException(ExceptionResources.ServiceConnectionError);
//                    if (OnEventErrorHandler != null)
//                        OnEventErrorHandler(ex, ErrorType.ERROR, "ESendService", new string[] { telegram.ToId.ToString() }, "SERVICE_CLIENT_TELEGRAMOUT", LinkById);
//                    throw ex;
//                }
//            }
//        }

//        /// <summary>
//        /// Удаление объекта клиента
//        /// </summary>
//        public void Dispose()
//        {
//            _log.Debug("Client was disposed.");
//            if (_connection != null)
//            {
//                try
//                {
//                    _connection.Socket.Close();
//                }
//                catch (Exception ex)
//                {
//                    if (OnEventErrorHandler != null)
//                        OnEventErrorHandler(ex, ErrorType.ERROR, "EUnknown", new string[] { string.Empty }, "SERVICE_CLIENT_DISPOSE", LinkById);
//                }
//            }
//        }

//        private void ReceiveCallback(object e)
//        {
//            var data = e as byte[];
//            if (data == null)
//                throw new DeveloperException(string.Format("Unknown telegram content. Wait byte array but recieved {0}.", e == null ? "null" : e.ToString()));
//            _log.DebugFormat("Client receive callback with data size = {0} bytes", data.Length);

//            // Читаем первый байт - он говорит что перед нами
//            var type = (TelegramBodyType)data[0];

//            // Читаем данные
//            var telegramData = data.Skip(1).ToArray();

//            // Читаем содержимое
//            var telegram = GSerialize.DeserializeBytes<Telegram>(telegramData);

//            // если системная телеграмма, то разбираем на месте
//            if (type == TelegramBodyType.Cmd)
//            {
//                _preProcessor.Parse(telegram);
//            }
//            else
//            {
//                var h = OnReceiveTelegram;
//                if (h != null)
//                    h(telegram);
//            }
//        }

//        private class ConnectionInfo
//        {
//            private const int MaxBuffersize = 1048576 * 200; //Max 2Gb

//            public ConnectionInfo()
//            {
//                Buffer = new byte[MaxBuffersize];
//            }

//            public Guid Id { get; set; }
//            public Socket Socket { get; set; }
//            public byte[] Buffer { get; set; }
//        }
//    }
//}