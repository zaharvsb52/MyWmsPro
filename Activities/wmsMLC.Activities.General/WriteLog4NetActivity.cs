using System.Activities;
using System.ComponentModel;
using log4net;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Activities.General
{
    public class WriteLog4NetActivity : NativeActivity
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(WriteLog4NetActivity));

        public WriteLog4NetActivity()
        {
            //Log4NetHelper.Configure(null);
            DisplayName = "Log4Net";
            Level = Log4NetLevel.Error;
        }

        [DisplayName(@"Уровень ошибок")]
        public Log4NetLevel Level { get; set; }

        [DisplayName(@"Ошибка (String)")]
        public InArgument<string> Error { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            var error = Error.Get(context);
            if (string.IsNullOrEmpty(error))
                return;
            var bpContext = GetBpContext(context);
            var errMsg = bpContext.Get<string>("ERROR");
            var newErrMsg = string.Format("{0}\r\n{1}", errMsg, error);
            bpContext.Set("ERROR", newErrMsg);

            switch (Level)
            {
                case Log4NetLevel.Debug:
                    _log.Debug(error);
                    return;
                case Log4NetLevel.Warning:
                    _log.Warn(error);
                    return;
                default:
                    _log.Error(error);
                    return;
            }
        }

        private static BpContext GetBpContext(NativeActivityContext context)
        {
            var datacontext = context.DataContext;
            var properties = datacontext.GetProperties();
            var bpcontextproperty = properties.Find(BpContext.BpContextArgumentName, true);
            if (bpcontextproperty == null)
                throw new DeveloperException("Неопределен аргумент типа BpContext.");

            return (BpContext)bpcontextproperty.GetValue(datacontext) ?? new BpContext();
        }
    }

    public enum Log4NetLevel
    {
        Error,
        Debug,
        Warning,
    }
}
