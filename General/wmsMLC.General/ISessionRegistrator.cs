using System;

namespace wmsMLC.General
{
    public interface ISessionRegistrator
    {
        decimal? CurrentSessionId { get; }
        string CurrentClientCode { get; }
        ClientTypeCode ClientType { get; }

        event EventHandler CurrentSessionIdChanged;

        event EventHandler SessionClosed;

        /// <summary>
        /// Проверяем наличие клиента clientcode, если отсутствует - добавляем. Создание клиентской сессии.
        /// </summary>
        void StartNewSession(string clientcode, ClientTypeCode clienttypecode, string clientname, string clientip, string clientmac, string clientSessionAppKey,
                         string clientSessionUserDomainName, string clientSessionClientVersion, string clientSessionServiceId, string clientOSVersion);

        void StartSession(string clientcode, ClientTypeCode clienttypecode, string clientip, string clientmac, string clientSessionAppKey, ref decimal? clientSessionId,
                         string clientSessionUserDomainName, string clientSessionClientVersion, string clientSessionServiceId, string clientOSVersion);

        /// <summary>
        /// Закрытие сессии клиента.
        /// </summary>
        void EndSession(decimal clientSessionId, bool clientSessionCorrectlyOff);

        /// <summary>
        /// Закрытие сессии клиента.
        /// </summary>
        void EndCurrentSession(bool clientSessionCorrectlyOff);

        /// <summary>
        /// Обновление сессии клиента.
        /// </summary>
        void UpdateSession(decimal clientSessionId, string clientSessionServiceId);

        /// <summary>
        /// Обновление сессии клиента.
        /// </summary>
        void UpdateCurrentSession(string clientSessionServiceId);

        /// <summary>
        /// Переоткрытие сессии клиента.
        /// </summary>
        void ReopenSession(decimal clientSessionId);
    }
}
