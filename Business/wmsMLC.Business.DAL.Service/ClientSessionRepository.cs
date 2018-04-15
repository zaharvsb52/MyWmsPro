using wmsMLC.Business.Objects;
using wmsMLC.General.BL;
using wmsMLC.General.DAL.Service;
using wmsMLC.General.DAL.Service.Telegrams;

namespace wmsMLC.Business.DAL.Service
{
    public abstract class ClientSessionRepository : BaseHistoryRepository<ClientSession, decimal>, IClientSessionRepository
    {
        public void StartSession(string clientcode, string clienttypecode, string clientip, string clientmac,
                                 string clientSessionAppKey, ref decimal? clientSessionId,
                                 string clientSessionUserDomainName, string clientSessionClientVersion, string clientSessionServiceId, string clientOSVersion)
        {
            var clientCodeParam = new TransmitterParam { Name = "clientcode", Type = typeof(string), Value = clientcode, IsOut = false };
            var clienttypecodeParam = new TransmitterParam { Name = "clienttypecode", Type = typeof(string), Value = clienttypecode, IsOut = false };
            var clientipParam = new TransmitterParam { Name = "clientip", Type = typeof(string), Value = clientip, IsOut = false };
            var clientmacParam = new TransmitterParam { Name = "clientmac", Type = typeof(string), Value = clientmac, IsOut = false };
            var clientSessionAppKeyParam = new TransmitterParam { Name = "clientSessionAppKey", Type = typeof(string), Value = clientSessionAppKey, IsOut = false };
            var clientSessionIdParam = new TransmitterParam { Name = "clientSessionId", Type = typeof(decimal?), Value = clientSessionId, IsOut = true };
            var clientSessionUserDomainNameParam = new TransmitterParam { Name = "clientSessionUserDomainName", Type = typeof(string), Value = clientSessionUserDomainName, IsOut = false };
            var clientSessionClientVersionParam = new TransmitterParam { Name = "clientSessionClientVersion", Type = typeof(string), Value = clientSessionClientVersion, IsOut = false };
            var clientSessionServiceIdParam = new TransmitterParam { Name = "clientSessionServiceId", Type = typeof(string), Value = clientSessionServiceId, IsOut = false };
            var clientOSVersionParam = new TransmitterParam { Name = "clientOSVersion", Type = typeof(string), Value = clientOSVersion, IsOut = false };

            var telegram = new RepoQueryTelegramWrapper(typeof(ClientSession).Name, "StartSession",
                new[] { clientCodeParam, clienttypecodeParam, clientipParam, clientmacParam, clientSessionAppKeyParam, clientSessionIdParam, 
                        clientSessionUserDomainNameParam, clientSessionClientVersionParam, clientSessionServiceIdParam, clientOSVersionParam });
            ProcessTelegramm(telegram);
            clientSessionId = clientSessionIdParam.Value == null
                ? (decimal?) null
                : (decimal) SerializationHelper.ConvertToTrueType(clientSessionIdParam.Value, typeof (decimal));
        }

        public void EndSession(decimal clientSessionId, bool clientSessionCorrectlyOff)
        {
            var clientSessionIdParam = new TransmitterParam { Name = "clientSessionId", Type = typeof(decimal), Value = clientSessionId, IsOut = false };
            var clientSessionCorrectlyOffParam = new TransmitterParam { Name = "clientSessionCorrectlyOff", Type = typeof(bool), Value = clientSessionCorrectlyOff, IsOut = false };
            var telegram = new RepoQueryNoAnswerTelegramWrapper(typeof (ClientSession).Name, "EndSession",
                new[] {clientSessionIdParam, clientSessionCorrectlyOffParam});
            ProcessTelegramm(telegram);
        }

        public void UpdateSession(decimal clientSessionId, string clientSessionServiceId)
        {
            var clientSessionIdParam = new TransmitterParam { Name = "clientSessionId", Type = typeof(decimal), Value = clientSessionId, IsOut = false };
            var clientSessionServiceIdParam = new TransmitterParam { Name = "clientSessionServiceId", Type = typeof(string), Value = clientSessionServiceId, IsOut = false };
            var telegram = new RepoQueryNoAnswerTelegramWrapper(typeof(ClientSession).Name, "UpdateSession",
                new[] { clientSessionIdParam, clientSessionServiceIdParam });
            ProcessTelegramm(telegram);
        }

        public void UpdateWorker(decimal clientSessionId, decimal? workerId)
        {
            var clientSessionIdParam = new TransmitterParam { Name = "clientSessionId", Type = typeof(decimal), Value = clientSessionId, IsOut = false };
            var workerIdParam = new TransmitterParam { Name = "workerId", Type = typeof(decimal?), Value = workerId, IsOut = false };
            var telegram = new RepoQueryNoAnswerTelegramWrapper(typeof(ClientSession).Name, "UpdateWorker",
                new[] { clientSessionIdParam, workerIdParam });
            ProcessTelegramm(telegram);
        }

    }
}
