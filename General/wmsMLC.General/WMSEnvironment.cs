using System;
using System.Collections.Generic;

namespace wmsMLC.General
{
    public sealed class WMSEnvironment
    {
        #region .  Singleton  .
        // ReSharper disable once InconsistentNaming
        private static readonly Lazy<WMSEnvironment> _instance = new Lazy<WMSEnvironment>(() => new WMSEnvironment { Settings = new Dictionary<string, object>() });
        private readonly ISessionRegistrator _sessionRegistrator;
        private readonly IAuthenticationProvider _authProvider;
        private bool? _isConnected;

        private readonly Lazy<DbSysInfo> _dbSystemInfo = new Lazy<DbSysInfo>(() =>
        {
            using (var mgr = IoC.Instance.Resolve<ISysDbInfo>())
                return mgr.DbSystemInfo;
        });


        public static WMSEnvironment Instance
        {
            get { return _instance.Value; }
        }

        private WMSEnvironment()
        {
            _sessionRegistrator = IoC.Instance.Resolve<ISessionRegistrator>();
            _authProvider = IoC.Instance.Resolve<IAuthenticationProvider>();
        }
        #endregion

        #region .  Properties  .
        /// <summary>
        /// ID cессии
        /// </summary>
        public decimal? SessionId
        {
            get { return _sessionRegistrator.CurrentSessionId; }
        }

        public string ClientCode
        {
            get { return _sessionRegistrator.CurrentClientCode; }
        }

        public ClientTypeCode ClientType { get { return _sessionRegistrator.ClientType; } }

        public IUserInfo AuthenticatedUser
        {
            get { return _authProvider.AuthenticatedUser; }
        }

        public bool? IsConnected
        {
            get
            {
                return _isConnected;
            }
            set
            {
                _isConnected = value;
                ConnectionFaultTime = DateTime.Now;
            }
        }

        public DateTime ConnectionFaultTime { get; private set; }

        public DbSysInfo DbSystemInfo
        {
            get
            {
                return _dbSystemInfo.Value;
            }
        }

        public decimal? WorkerId { get; set; }

        public string TruckCode { get; set; }

        public string SdclCode { get; set; }

        public string EndPoint { get; set; }

        #endregion

        #region .  Session settings  .
        /// <summary>
        /// Дополнительные свойства БП.
        /// </summary>
        public IDictionary<string, object> Settings { get; private set; }
        #endregion

        #region .  Methods  .

        public T Get<T>(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            try
            {
                return (T) Convert.ChangeType(Settings[name], typeof (T));
            }
            catch (InvalidCastException ex)
            {
                // переобернем только ошибки конвертаци - остальное бросаем дальше
                throw new OperationException(Resources.ExceptionResources.TypeConversionError, "WMSEnvironment.", name, typeof(T), ex);
            }
        }

        public T TryGet<T>(string name)
        {
            if (string.IsNullOrEmpty(name) || !Settings.ContainsKey(name))
                return default(T);
            return Get<T>(name);
        }

        public void Set<T>(string name, T value)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            Settings[name] = value;
        }

        #endregion
    }
}