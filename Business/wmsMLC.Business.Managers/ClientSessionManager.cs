using System;
using System.Linq;
using wmsMLC.Business.DAL;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Business.Managers
{
    public class ClientSessionManager : WMSBusinessObjectManager<ClientSession, decimal>, ISessionRegistrator, IClientSessionManager
    {
        public ClientSessionManager()
        {
            CurrentClientCode = Environment.MachineName;
        }

        private decimal? _currentSessionId;
        public decimal? CurrentSessionId
        {
            get { return _currentSessionId; }
            private set
            {
                if (_currentSessionId == value)
                    return;

                _currentSessionId = value;
                OnCurrentSessionChanged();
            }
        }

        public string CurrentClientCode { get; private set; }
        public ClientTypeCode ClientType { get; private set; }

        public event EventHandler CurrentSessionIdChanged;

        public event EventHandler SessionClosed;

        private void OnCurrentSessionChanged()
        {
            var h = CurrentSessionIdChanged;
            if (h != null)
                h(this, EventArgs.Empty);
        }

        private void OnSessionClosed(decimal clientSessionId)
        {
            var h = SessionClosed;
            if (h != null)
                h(clientSessionId, EventArgs.Empty);
        }

        public void StartSession(string clientcode, ClientTypeCode clienttypecode, string clientip, string clientmac,
                                 string clientSessionAppKey, ref decimal? clientSessionId,
                                 string clientSessionUserDomainName, string clientSessionClientVersion, string clientSessionServiceId, string clientOSVersion)
        {
            using (var repo = GetRepository<IClientSessionRepository>())
                repo.StartSession(clientcode: clientcode, clienttypecode: clienttypecode.ToString(), clientip: clientip,
                                  clientmac: clientmac, clientSessionAppKey: clientSessionAppKey,
                                  clientSessionId: ref clientSessionId,
                                  clientSessionUserDomainName: clientSessionUserDomainName, clientSessionClientVersion: clientSessionClientVersion,
                                  clientSessionServiceId: clientSessionServiceId, clientOSVersion: clientOSVersion);
            CurrentClientCode = clientcode;
            CurrentSessionId = clientSessionId;
            ClientType = clienttypecode;
        }

        public void StartNewSession(string clientcode, ClientTypeCode clienttypecode, string clientname, string clientip,
                                 string clientmac, string clientSessionAppKey,
                                 string clientSessionUserDomainName, string clientSessionClientVersion, string clientSessionServiceId, string clientOSVersion)
        {
            var clientSessionId = CurrentSessionId;
            StartSession(clientcode: clientcode, clienttypecode: clienttypecode, clientip: clientip,
                         clientmac: clientmac, clientSessionAppKey: clientSessionAppKey,
                         clientSessionId: ref clientSessionId,
                         clientSessionUserDomainName: clientSessionUserDomainName, clientSessionClientVersion: clientSessionClientVersion,
                         clientSessionServiceId: clientSessionServiceId, clientOSVersion: clientOSVersion);
        }

        public void EndSession(decimal clientSessionId, bool clientSessionCorrectlyOff)
        {
            using (var repo = GetRepository<IClientSessionRepository>())
                repo.EndSession(clientSessionId, clientSessionCorrectlyOff);
            if (clientSessionCorrectlyOff)
                OnSessionClosed(clientSessionId);
            CurrentSessionId = null;
        }

        public void EndCurrentSession(bool clientSessionCorrectlyOff)
        {
            if (!CurrentSessionId.HasValue)
                throw new DeveloperException("Невозможно завершить сессию. Нет активных сессий.");

            EndSession(CurrentSessionId.Value, clientSessionCorrectlyOff);
        }

        public void UpdateSession(decimal clientSessionId, string clientSessionServiceId)
        {
            using (var repo = GetRepository<IClientSessionRepository>())
                repo.UpdateSession(clientSessionId, clientSessionServiceId);
        }

        public void UpdateCurrentSession(string clientSessionServiceId)
        {
            if (!CurrentSessionId.HasValue)
                throw new DeveloperException("Невозможно завершить сессию. Нет активных сессий.");
            UpdateSession(CurrentSessionId.Value, clientSessionServiceId);
        }

        public string GetClientInputPlaceValue()
        {
            if (WMSEnvironment.Instance.SessionId == null)
                return null;

            var session = Get((decimal)WMSEnvironment.Instance.SessionId);
            if (session == null)
                return null;

            Client client;
            using (var mgrClient = GetManager<Client>())
            {
                // TODO: что это?
                if (mgrClient == null)
                    return string.Empty;

                client = mgrClient.Get(session.ClientCode_R);
            }

            if (client == null)
                return string.Empty;

            var cpv = client.GetProperty<WMSBusinessCollection<ClientCpv>>("CustomParamVal");
            if (cpv == null)
                return string.Empty;

            var clientPlace = cpv.FirstOrDefault(i => i.CustomParamCode == "ClientInputPlaceL2");
            if (clientPlace == null || string.IsNullOrEmpty(clientPlace.CPVValue))
                return string.Empty;

            return clientPlace.CPVValue;
        }

        public bool CloseRclSession(string clientCode, string truckCode)
        {
            if (string.IsNullOrEmpty(clientCode))
                throw new ArgumentNullException("clientCode");

            //Получаем открытые сессии RCL для клиента clientCode
            var type = typeof (ClientSession);

            var sessions = GetFiltered(string.Format("{0} = '{1}' AND upper({2}) = 'RCL' AND {3} IS NULL AND NVL({4}, 'A') <> NVL('{5}', 'A') AND {6} >= sysdate - 2",
                SourceNameHelper.Instance.GetPropertySourceName(type, ClientSession.ClientCode_RPropertyName),
                clientCode,
                SourceNameHelper.Instance.GetPropertySourceName(type, ClientSession.ClientTypeCode_RPropertyName),
                SourceNameHelper.Instance.GetPropertySourceName(type, ClientSession.ClientSessionEndPropertyName),
                SourceNameHelper.Instance.GetPropertySourceName(type, ClientSession.TruckCode_RPropertyName),
                truckCode,
                SourceNameHelper.Instance.GetPropertySourceName(type, ClientSession.ClientSessionBeginPropertyName)),
                GetModeEnum.Partial).ToArray();

            if (sessions.Length == 0)
                return false;

            using (var repo = GetRepository<IClientSessionRepository>())
            {
                foreach (var s in sessions)
                {
                    repo.EndSession(s.GetKey<decimal>(), false);
                }
            }

            return true;
        }

        public void UpdateWorker(decimal clientSessionId, decimal? workerId)
        {
            var clses = Get(clientSessionId);
            if (clses == null)
                return;

            if ((clses.WorkerID_R == null && workerId.HasValue) ||
                (workerId.HasValue && clses.WorkerID_R != workerId.Value))
            {
                using (var repo = GetRepository<IClientSessionRepository>())
                    repo.UpdateWorker(clientSessionId, workerId);
            }
        }

        public void ReopenSession(decimal clientSessionId)
        {
            var session = Get(clientSessionId);
            if (session == null)
                throw new DeveloperException("Can't find ClientSession by key '{0}'.", clientSessionId);
            
            if (!session.ClientSessionEnd.HasValue)
                return;

            session.ClientSessionCorrectlyOff = null;
            session.ClientSessionEnd = null;
            Update(session);
        }
    }
}