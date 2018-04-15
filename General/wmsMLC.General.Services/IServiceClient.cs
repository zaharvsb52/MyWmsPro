namespace wmsMLC.General.Services
{
    public interface IServiceClient
    {
        #region .  Properties  .

        /// <summary> Идентификатор сервиса </summary>
        string ServiceId { get; }

        /// <summary> Номер сессии (ProcessId). </summary>
        int Session { get; }

        /// <summary> Id-адрес клиента. </summary>
        string IpAddress { get; }

        /// <summary> Mac-адрес клиента. </summary>
        string MacAddress { get; }

        /// <summary> Тип клиента (ID). </summary>
        ClientTypeCode ClientType { get; }

        string HostName { get; }

        /// <summary> Точка соединения с сервисом </summary>
        string EndPoint { get; }

        #endregion

        Telegram Process(TelegramBodyType type, Telegram telegram);

        /// <summary>
        /// Переподключение к указанному сервису
        /// </summary>
        /// <param name="info"></param>
        void Reconnect(SdclConnectInfo info);
    }
}
