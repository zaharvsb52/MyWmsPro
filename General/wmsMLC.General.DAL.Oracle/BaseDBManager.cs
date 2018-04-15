using System;
using System.Data;
using System.Text;
using System.Xml;
using BLToolkit.Data;
using BLToolkit.Data.DataProvider;
using log4net;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

namespace wmsMLC.General.DAL.Oracle
{
    /// <summary>
    /// Abstraction on DbManager.
    /// </summary>
    public class BaseDBManager : DbManager
    {
        #region .  Fields & Consts  .
        private const string LogPrefTemplate = "ORACLE:{0}:{1}:";
        private const string LogParamTemplate = "Parameter {0}='{1}'";
        private const string NullWord = "NULL";
        private const string CMDWord = "CMD=";

        private readonly ILog _log = LogManager.GetLogger(typeof(BaseDBManager));
        private DateTime _openConnectionDate;
        private bool _inSetSessionParameters;
        private string _logPref;

        public static int CommandTimeout = 0;
        public static int InitialLOBFetchSize = -1;
        #endregion

        #region .  Constructors  .
        static BaseDBManager()
        {
            CommandTimeout = Properties.Settings.Default.CommandTimeout;
            InitialLOBFetchSize = Properties.Settings.Default.InitialLOBFetchSize;
        }
        public BaseDBManager() { Initilize(); }
        public BaseDBManager(string configurationString) : base(configurationString) { Initilize(); }
        public BaseDBManager(string providerName, string configuration) : base(providerName, configuration) { Initilize(); }
        public BaseDBManager(IDbConnection connection) : base(connection) { Initilize(); }
        public BaseDBManager(IDbTransaction transaction) : base(transaction) { Initilize(); }
        public BaseDBManager(DataProviderBase dataProvider, string connectionString) : base(dataProvider, connectionString) { Initilize(); }
        public BaseDBManager(DataProviderBase dataProvider, IDbConnection connection) : base(dataProvider, connection) { Initilize(); }
        public BaseDBManager(DataProviderBase dataProvider, IDbTransaction transaction) : base(dataProvider, transaction) { Initilize(); }

        #endregion

        #region .  Properties  .
        public string UserSignature { get; private set; }

        /// <summary>
        /// Таймаут выполнения комманды в милисекундах
        /// </summary>
        public int? TimeOut { get; set; }

        public int? SessionId { get; private set; }
        #endregion

        #region .  Methods  .
        public void SetParametersFromUoW(UnitOfWork unitOfWork)
        {
            TimeOut = unitOfWork.TimeOut;
            SessionId = unitOfWork.SessionId;
            UserSignature = unitOfWork.UserSignature;
        }

        private void Initilize() { }

        private static string GetParamValueString(IDbDataParameter p, bool withInnerData = false)
        {
            if (p.Value == null)
                return NullWord;

            if (p.Value is TLISTXML)
            {
                if (!withInnerData)
                    return "TLISTXML";

                var val = (TLISTXML)p.Value;
                var sb = new StringBuilder(val.ToString());
                foreach (var s in val.Value)
                {
                    sb.Append(s.InnerXml);
                }
                return sb.ToString();
            }

            if (p.Value is OracleXmlType)
            {
                if (!withInnerData)
                    return "OracleXmlType";

                return ((OracleXmlType)p.Value).Value;
            }

            if (p.Value is XmlDocument)
            {
                if (!withInnerData)
                    return "XmlDocument";

                return ((XmlDocument)p.Value).InnerXml;
            }

            if (p.Value is OracleBlob)
                return "OracleBlob";

            if (p.Value is OracleClob)
                return "OracleClob";

            // простая защита от вывода в лог паролей
            if (p.ParameterName.ToLower().Contains("pass") ||
                p.ParameterName.ToLower().Contains("credent"))
                return "***";

            return p.Value.ToString();
        }

        private string GetLogPref()
        {
            if (!string.IsNullOrEmpty(_logPref))
                return _logPref;

            _logPref = string.Format(LogPrefTemplate,
                (string.IsNullOrEmpty(UserSignature) ? NullWord : UserSignature),
                (SessionId.HasValue ? SessionId.ToString() : NullWord))
                + "{0}";
            return _logPref;
        }

        protected override void OnBeforeOperation(OperationType op)
        {
            if (_log.IsDebugEnabled && !_inSetSessionParameters)
            {
                var logPref = GetLogPref();
                switch (op)
                {
                    case OperationType.Fill:
                    case OperationType.ExecuteReader:
                    case OperationType.ExecuteNonQuery:
                        {
                            var command = this.Command;
                            _log.DebugFormat(logPref, CMDWord + command.CommandText);
                            if (command.Parameters != null)
                            {
                                foreach (var p in command.Parameters)
                                {
                                    var typedParam = ((IDbDataParameter)p);
                                    var value = GetParamValueString(typedParam);
                                    _log.DebugFormat(logPref, string.Format(LogParamTemplate, typedParam.ParameterName, value));
                                }
                            }
                        }
                        break;
                    case OperationType.OpenConnection:
                        _log.DebugFormat(logPref, op);
                        _openConnectionDate = DateTime.Now;
                        break;
                    case OperationType.CloseConnection:
                        _log.DebugFormat(logPref, string.Format("{0} duration={1}", op, (DateTime.Now - _openConnectionDate)));
                        break;
                    default:
                        _log.DebugFormat(logPref, op);
                        break;
                }
            }

            base.OnBeforeOperation(op);
        }

        protected override void OnAfterOperation(OperationType op)
        {
            if (op == OperationType.OpenConnection)
                SetSessionParameters(UserSignature, SessionId);

            base.OnAfterOperation(op);
        }

        protected override IDbCommand OnInitCommand(IDbCommand command)
        {
            var cmd = base.OnInitCommand(command);

            var oraCmd = (OracleCommand)cmd;

            if (oraCmd.InitialLOBFetchSize != InitialLOBFetchSize)
                oraCmd.InitialLOBFetchSize = InitialLOBFetchSize;

            if (TimeOut.HasValue)
            {
                var timeOutInSec = (int)Math.Truncate((decimal)(TimeOut.Value) / 1000);

                if (oraCmd.CommandTimeout != timeOutInSec)
                    oraCmd.CommandTimeout = timeOutInSec;
            }
            else
                if (oraCmd.CommandTimeout != CommandTimeout)
                    oraCmd.CommandTimeout = CommandTimeout;

            return cmd;
        }

        /// <summary>
        /// Представляемся системе (передаем параметр client_id). Данный параметр используется для аудита действий пользователей
        /// </summary>
        /// <param name="userCode">client_id</param>
        /// <param name="sessionId">код сессий</param>
        private void SetSessionParameters(string userCode, int? sessionId)
        {
            if (_inSetSessionParameters)
                return;

            try
            {
                _inSetSessionParameters = true;

                // запускаем отдельно (чистым ADO)
                // есть проблемы с запуском через BLToolkit именно в этом месте
                using (var cmd = Connection.CreateCommand())
                {
                    var pUserCode = new OracleParameter("pusercode", OracleDbType.NVarchar2) {Value = userCode};
                    var pSessionId = new OracleParameter("psessionid", OracleDbType.Int32) {Value = sessionId};

                    cmd.CommandText = "set_sessionparam";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(pUserCode);
                    cmd.Parameters.Add(pSessionId);

                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                _inSetSessionParameters = false;
            }
        }
        #endregion
    }
}