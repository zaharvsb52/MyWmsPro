using wmsMLC.Business.Objects;
using wmsMLC.General.DAL;

namespace wmsMLC.Business.DAL
{
    public interface IClientSessionRepository : IRepository<ClientSession, decimal>
    {
        void StartSession(string clientcode, string clienttypecode, string clientip, string clientmac,
                          string clientSessionAppKey, ref decimal? clientSessionId,
                          string clientSessionUserDomainName, string clientSessionClientVersion, string clientSessionServiceId, string clientOSVersion);

        void EndSession(decimal clientSessionId, bool clientSessionCorrectlyOff);

        /// <summary>
        /// Обновление сессии клиента.
        /// </summary>
        void UpdateSession(decimal clientSessionId, string clientSessionServiceId);

        void UpdateWorker(decimal clientSessionId, decimal? workerId);
    }
}
