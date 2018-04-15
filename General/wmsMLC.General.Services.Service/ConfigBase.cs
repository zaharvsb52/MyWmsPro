using wmsMLC.General.BL;

namespace wmsMLC.General.Services.Service
{
    /// <summary>
    /// Базовый класс для конфигурации сервиса
    /// </summary>
    public abstract class ConfigBase
    {
        private readonly ServiceContext _context;

        #region . Constants .

        public static readonly string NameParam = "name";
        public static readonly string HandlerParam = "handler";
        public static readonly string SendCountParam = "sendcount";
        public static readonly string DebugLevelParam = "debuglevel";
        public static readonly string EnvironmentParam = "env";

        #endregion

        protected ConfigBase(ServiceContext context)
        {
            _context = context;
        }

        public string Name
        {
            get
            {
                return Get<string>(NameParam);
            }
        }

        public string Environment
        {
            get
            {
                return Get<string>(EnvironmentParam);
            }
        }

        public int Handler
        {
            get
            {
                return Get<int>(HandlerParam);
            }
        }

        public T Get<T>(string paramName)
        {
            if(_context == null)
                throw new OperationException("ServiceContext is null");

            var value = _context.Get(paramName);
            if (string.IsNullOrEmpty(value))
                return default(T);

            return (T)SerializationHelper.ConvertToTrueType(value, typeof (T));
        }
    }
}
