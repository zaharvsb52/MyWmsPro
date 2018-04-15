using System;
using System.Data;
using Oracle.DataAccess.Client;
using wmsMLC.Business.Objects;

namespace wmsMLC.Business.DAL.Oracle
{
    public abstract class ClientSessionRepository : BaseHistoryRepository<ClientSession, decimal>, IClientSessionRepository
    {
        public void StartSession(string clientcode, string clienttypecode, string clientip, string clientmac, string clientSessionAppKey, ref decimal? clientSessionId,
                                 string clientSessionUserDomainName, string clientSessionClientVersion, string clientSessionServiceId, string clientOSVersion)
        {
            StartSessionApi(clientcode: clientcode, clienttypecode: clienttypecode, clientip: clientip,
                            clientmac: clientmac, clientSessionAppKey: clientSessionAppKey,
                            clientSessionId: ref clientSessionId,
                            clientSessionUserDomainName: clientSessionUserDomainName, clientSessionClientVersion: clientSessionClientVersion, 
                            clientSessionServiceId: clientSessionServiceId, clientOSVersion: clientOSVersion);
        }

        public void EndSession(decimal clientSessionId, bool clientSessionCorrectlyOff)
        {
            EndSessionApi(clientSessionId, clientSessionCorrectlyOff ? 1 : 0);
        }

        public void UpdateSession(decimal clientSessionId, string clientSessionServiceId)
        {
            UpdateSessionApi(clientSessionId, clientSessionServiceId);
        }

        public void UpdateWorker(decimal clientSessionId, decimal? workerId)
        {
            UpdateWorkerApi(clientSessionId, workerId);
        }

        #region . DB API .

        public virtual void StartSessionApi(string clientcode, string clienttypecode, string clientip, string clientmac, string clientSessionAppKey, ref decimal? clientSessionId,
                                 string clientSessionUserDomainName, string clientSessionClientVersion, string clientSessionServiceId, string clientOSVersion)
        {
            decimal? id = clientSessionId;
            id = RunManualDbOperation<decimal?>(db =>
            {
                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pClientCode", OracleDbType.Varchar2, clientcode, ParameterDirection.Input),
                    new OracleParameter("pClientTypeCode", OracleDbType.Varchar2, clienttypecode, ParameterDirection.Input),
                    new OracleParameter("pClientIP", OracleDbType.Varchar2, clientip, ParameterDirection.Input),
                    new OracleParameter("pClientMAC", OracleDbType.Varchar2, clientmac, ParameterDirection.Input),
                    new OracleParameter("pClientSessionAppKey", OracleDbType.Varchar2, clientSessionAppKey, ParameterDirection.Input),
                    new OracleParameter("pClientSessionID", OracleDbType.Int32, id, ParameterDirection.InputOutput),
                    new OracleParameter("pClientSessionUserDomainName", OracleDbType.Varchar2, clientSessionUserDomainName, ParameterDirection.Input),
                    new OracleParameter("pClientSessionClientVersion", OracleDbType.Varchar2, clientSessionClientVersion, ParameterDirection.Input),
                    new OracleParameter("pClientSessionServiceId", OracleDbType.Varchar2, clientSessionServiceId, ParameterDirection.Input),
                    new OracleParameter("pClientOSVersion", OracleDbType.Varchar2, clientOSVersion, ParameterDirection.Input)
                };
                db.SetCommand(CommandType.StoredProcedure, "pkgBpProcess.startSession", ps).ExecuteNonQuery();
                var result = (OracleParameter)ps.GetValue(5);
                return (result.Value == null) ? (decimal?)null : Convert.ToDecimal(result.Value.ToString());
            });
            clientSessionId = id;
        }

        public virtual void EndSessionApi(decimal? clientSessionId, int? clientSessionCorrectlyOff)
        {
            RunManualDbOperation(db =>
            {
                var ps = new IDbDataParameter[]
                {
                    new OracleParameter("pClientSessionID", OracleDbType.Int32, clientSessionId, ParameterDirection.Input),
                    new OracleParameter("pClientSessionCorrectlyOff", OracleDbType.Int32, clientSessionCorrectlyOff, ParameterDirection.Input)
                };
                return db.SetCommand(CommandType.StoredProcedure, "pkgBpProcess.endSession", ps).ExecuteNonQuery();
            });
        }

        public virtual void UpdateSessionApi(decimal? clientSessionId, string clientSessionServiceId)
        {
            RunManualDbOperation(db =>
                {
                    var ps = new IDbDataParameter[]
                        {
                            new OracleParameter("pClientSessionID", OracleDbType.Int32, clientSessionId, ParameterDirection.Input),
                            new OracleParameter("pClientSessionServiceId", OracleDbType.Varchar2, clientSessionServiceId, ParameterDirection.Input)
                        };
                return db.SetCommand(CommandType.StoredProcedure, "pkgBpProcess.updateSession", ps).ExecuteNonQuery();
            });
        }

        public virtual void UpdateWorkerApi(decimal clientSessionId, decimal? workerId)
        {
            RunManualDbOperation(db =>
            {
                var ps = new IDbDataParameter[]
                        {
                            new OracleParameter("pClientSessionID", OracleDbType.Int32, clientSessionId, ParameterDirection.Input),
                            new OracleParameter("pWorkerId", OracleDbType.Int32, workerId, ParameterDirection.Input)
                        };
                return db.SetCommand(CommandType.StoredProcedure, "pkgBpProcess.updateSession", ps).ExecuteNonQuery();
            });
        }

        #endregion . DB API .
    }
}

//[DiscoverParameters(false)]
//[SprocName("pkgBpProcess.startSession")]
//public abstract void StartSessionApi([ParamName("ClientCode")] string clientcode,
//                                     [ParamName("ClientTypeCode")] string clienttypecode,
//                                     [ParamName("ClientIP")] string clientip,
//                                     [ParamName("ClientMAC")] string clientmac,
//                                     [ParamName("ClientSessionAppKey")] string clientSessionAppKey,
//                                     [ParamName("ClientSessionID")] ref decimal? clientSessionId);
//[DiscoverParameters(false)]
//[SprocName("pkgBpProcess.endSession")]
//public abstract void EndSessionApi([ParamName("ClientSessionID")] decimal? clientSessionId,
//                                   [ParamName("ClientSessionCorrectlyOff")] int? clientSessionCorrectlyOff);
//[DiscoverParameters(false)]
//[SprocName("pkgBpProcess.updateSession")]
//public abstract void UpdateSessionApi([ParamName("ClientSessionID")] decimal? clientSessionId);
