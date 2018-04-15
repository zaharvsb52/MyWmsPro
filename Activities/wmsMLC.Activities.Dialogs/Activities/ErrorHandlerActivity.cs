using System;
using System.Activities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using log4net;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.Activities.Dialogs.Activities
{
    [DisplayName(@"Обработчик ошибок")]
    public class ErrorHandlerActivity : NativeActivity<Exception>
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(ErrorHandlerActivity));

        public ErrorHandlerActivity()
        {
            DisplayName = "Обработчик ошибок";
            ShowErorDialog = true;
            IsActivitiesStackIncluded = true;
            ForceError = false;
            IsSendMail = false;
            Rethrow = false;
        }

        #region .  Properties  .
        [DisplayName(@"Показать сообщение об ошибке")]
        [DefaultValue(true)]
        public InArgument<bool> ShowErorDialog { get; set; }

        /// <summary>
        /// Заголовок.
        /// </summary>
        [DisplayName(@"Заголовок")]
        public InArgument<string> Title { get; set; }

        /// <summary>
        /// Сообщение.
        /// </summary>
        [DisplayName(@"Сообщение")]
        public InArgument<string> Message { get; set; }

        [DisplayName(@"Ошибка (Exception)")]
        public InArgument<Exception> Error { get; set; }

        [DisplayName(@"Показать как ошибку")]
        [DefaultValue(false)]
        public InArgument<bool> ForceError { get; set; }

        [DisplayName(@"Включить трасировку Activites")]
        [DefaultValue(true)]
        public InArgument<bool> IsActivitiesStackIncluded { get; set; }

        [DisplayName(@"Отправлять уведомление об ошибке на почту")]
        [DefaultValue(false)]
        public InArgument<bool> IsSendMail { get; set; }

        [DisplayName(@"Пересоздать ошибку")]
        [DefaultValue(false)]
        public InArgument<bool> Rethrow { get; set; }
        #endregion .  Properties  .

        #region .  Methods  .
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, ShowErorDialog, type.ExtractPropertyName(() => ShowErorDialog));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Title, type.ExtractPropertyName(() => Title));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Message, type.ExtractPropertyName(() => Message));
            ActivityHelpers.AddCacheMetadata(collection, metadata, ForceError, type.ExtractPropertyName(() => ForceError));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Error, type.ExtractPropertyName(() => Error));
            ActivityHelpers.AddCacheMetadata(collection, metadata, IsActivitiesStackIncluded, type.ExtractPropertyName(() => IsActivitiesStackIncluded));
            ActivityHelpers.AddCacheMetadata(collection, metadata, IsSendMail, type.ExtractPropertyName(() => IsSendMail));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Rethrow, type.ExtractPropertyName(() => Rethrow));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            //Отладка
            //var tracking = new CustomTrackingRecord(DisplayName);
            //context.Track(tracking);

            //Подготавливаем activities stack
            var activitiesStack = GetActivitiesStack(context);

            // собираем ошибку
            var error = Error.Get(context);
            var sbMessage = new StringBuilder();
            // пишем только уникальные ошибки
            var trueEx = ExceptionHelper.GetFirstMeaningException(error);
            var errMessage = ExceptionHelper.GetErrorMessage(trueEx);
            if (sbMessage.Length == 0 || !sbMessage.ToString().Contains(errMessage))
                sbMessage.AppendLine(errMessage);
            var totalErrorMessage = sbMessage.ToString();
            
            var logTotalErrorMessage = totalErrorMessage.Trim();

            var title = Title.Get(context);
            var message = Message.Get(context);
            var exception = error ?? new OperationException(logTotalErrorMessage);

            //Отправка почты
            if (IsSendMail.Get(context))
            {
                try
                {
                    //TODO: Отправка по EMAIL
                }
                catch (Exception exc)
                {
                    _log.Error("ErrorHandlerActivity -> send mail error", exc);
                }
            }

            var showErorDialog = ShowErorDialog.Get(context);
            var vs = showErorDialog ? IoC.Instance.Resolve<IViewService>() : null;
            Result.Set(context, string.IsNullOrEmpty(activitiesStack) ? exception : new Exception(activitiesStack, exception));

            if (!ForceError.Get(context) && (error is ICustomException || (error != null && error.InnerException is ICustomException)))
            {
                _log.Warn(string.IsNullOrEmpty(activitiesStack)
                    ? logTotalErrorMessage
                    : string.Format("{0}{1}{2}", logTotalErrorMessage, Environment.NewLine, activitiesStack), error);

                if (showErorDialog)
                {
                    var dlgmessage = string.IsNullOrEmpty(message) || totalErrorMessage.Contains(message)
                        ? totalErrorMessage
                        : string.Format("{0}{1}{2}", message, Environment.NewLine, totalErrorMessage);
                    vs.ShowDialog(title, dlgmessage, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(activitiesStack))
                    _log.Error(exception);
                else
                    _log.Error(string.Format("{0}: {1}{2}{3}", exception.GetType().FullName, exception.Message, Environment.NewLine,
                        activitiesStack), exception);

                if (showErorDialog)
                    vs.ShowError(message, exception);
            }

            if (exception != null && Rethrow.Get(context))
                throw exception;
        }

        private string GetActivitiesStack(ActivityContext context)
        {
            if (!IsActivitiesStackIncluded.Get(context))
                return null;

            var ext = ActivityHelpers.GetTraceExtension(context);
            if (ext == null || ext.NotUseActivityStackTrace)
                return null;

            var activityStackTrace = ext.Get<ActivityStackTrace>(TraceExtension.ActivityTrackingSourcePropertyName);
            if (activityStackTrace == null)
                return null;

            const string format = "   {0} {1}";
            const string instr = "in";
            const string errorstr = "error " + instr;
            var ast = new StringBuilder();

            var isFaultSource = activityStackTrace.FaultSource == null;
            var isFaultHandler = activityStackTrace.FaultHandler == null;

            //Добавляем в stack текущую activity
            var carrentActivityInfo = new InfoOfActivity(name: DisplayName, id: Id,
                instanceId: context.ActivityInstanceId,
                typeName: GetType().FullName);
            if (activityStackTrace.Activities.All(a => a.CompareTo(carrentActivityInfo) != 0))
                ast.AppendLine(string.Format(format, instr, carrentActivityInfo));

            var count = activityStackTrace.Activities.Count;
            for (var i = count - 1; i >= 0; i--)
            {
                var a = activityStackTrace.Activities[i];
                if (!isFaultSource && activityStackTrace.FaultSource.CompareTo(a) == 0)
                {
                    isFaultSource = true;
                    ast.AppendLine(string.Format(format, errorstr, a));
                    continue;
                }

                if (!isFaultHandler && activityStackTrace.FaultHandler.CompareTo(a) == 0)
                    isFaultHandler = true;

                ast.AppendLine(string.Format(format, instr, a));
            }

            if (!isFaultSource)
                ast.AppendLine(string.Format(format, "? -> " + errorstr, activityStackTrace.FaultSource));

            if (!isFaultHandler)
            {
                if (count > 0)
                    ast.AppendLine(string.Format(format, "...", null));
                ast.AppendLine(string.Format(format, instr, activityStackTrace.FaultHandler));
            }

            if (ast.Length > 0)
                ast.Append("--- End of stack trace activities ---");
            var result = ast.ToString();

            //Чистим ошибку
            activityStackTrace.ClearFault();
            return result;
        }
        #endregion .  Methods  .
    }
}
