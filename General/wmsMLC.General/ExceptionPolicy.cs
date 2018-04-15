using System;
using System.Threading.Tasks;

namespace wmsMLC.General
{
    /// <summary>
    /// Класс предоставляющий единую точку доступа для обработки исключений в системе.
    /// <remarks>MS Exception Handling Block не использовался, т.к. для нас избыточен</remarks>
    /// </summary>
    public sealed class ExceptionPolicy
    {
        #region .  Singleton initialization  .
        private static readonly Lazy<ExceptionPolicy> _inst = new Lazy<ExceptionPolicy>(() => new ExceptionPolicy());

        private ExceptionPolicy()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                HandleException((Exception)args.ExceptionObject, typeof(ExceptionPolicy));
            };

            // подписываемся на необработанные ошибки в TASK-ах - http://lambsoftware.wordpress.com/2012/04/04/handling-unhandled-exceptions-in-net-4-0/
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
        }

        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs args)
        {
            args.SetObserved();
            HandleException(args.Exception, typeof(ExceptionPolicy));
        }

        public static ExceptionPolicy Instance
        {
            get { return _inst.Value; }
        }
        #endregion

        public void Init()
        {
            // можно ничего не делать - просто создадим инстанс и подпишемся на UnhandledException
        }

        /// <summary>
        /// Обработать исключение
        /// </summary>
        /// <param name="ex">Возникшее исключение</param>
        /// <param name="catcherType">Тип объекта, в котором возникло исключение</param>
        /// <returns>Истина - если ошибка обработана и не требуется ее проброс</returns>
        public bool HandleException(Exception ex, Type catcherType)
        {
            return HandleException(ex, catcherType.Name);
        }

        /// <summary>
        /// Обработать исключение
        /// </summary>
        /// <param name="ex">Возникшее исключение</param>
        /// <param name="policyName">Имя политики обработки</param>
        /// <returns>Истина - если ошибка обработана и не требуется ее проброс</returns>
        public bool HandleException(Exception ex, string policyName)
        {
            var args = new ExceptionEventArgs(ex, policyName);

            OnExceptionOccure(args);

            return !args.NeedThrow;
        }

        public ExceptionHandleResult Handle(Exception ex, string policyName)
        {
            return new ExceptionHandleResult();
        }

        /// <summary> Событие о возникновении ошибки </summary>
        public event EventHandler<ExceptionEventArgs> ExceptionOccure;

        private void OnExceptionOccure(ExceptionEventArgs e)
        {
            var handler = ExceptionOccure;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Преобразование исключения в строку. Использование этого метода обеспечит единый формат вывода
        /// </summary>
        /// <param name="ex">Исключение</param>
        /// <returns>Строковое описание исключения</returns>
        [Obsolete("Use ExceptionHelper.ExceptionToString instead")]
        public static string ExceptionToString(Exception ex)
        {
            //TODO: изменить все вызовы сразу на ExceptionHelper
            return ExceptionHelper.ExceptionToString(ex);
        }
    }

    public class ExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; set; }
        public string PolicyName { get; private set; }
        public bool Handled { get; set; }
        public bool NeedThrow { get; set; }

        public ExceptionEventArgs(Exception exception, string policyName)
        {
            Exception = exception;
            PolicyName = policyName;
            NeedThrow = true;
        }
    }

    public class ExceptionHandleResult
    {
        public Exception Exception { get; set; }
        public ExceptionHandleAction Action { get; set; }
    }

    public enum ExceptionHandleAction
    {
        ReThrow,
        ReTry,
        Ignore
    }
}