using System;
using System.Threading.Tasks;

namespace wmsMLC.General
{
    /// <summary>
    /// ����� ��������������� ������ ����� ������� ��� ��������� ���������� � �������.
    /// <remarks>MS Exception Handling Block �� �������������, �.�. ��� ��� ���������</remarks>
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

            // ������������� �� �������������� ������ � TASK-�� - http://lambsoftware.wordpress.com/2012/04/04/handling-unhandled-exceptions-in-net-4-0/
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
            // ����� ������ �� ������ - ������ �������� ������� � ���������� �� UnhandledException
        }

        /// <summary>
        /// ���������� ����������
        /// </summary>
        /// <param name="ex">��������� ����������</param>
        /// <param name="catcherType">��� �������, � ������� �������� ����������</param>
        /// <returns>������ - ���� ������ ���������� � �� ��������� �� �������</returns>
        public bool HandleException(Exception ex, Type catcherType)
        {
            return HandleException(ex, catcherType.Name);
        }

        /// <summary>
        /// ���������� ����������
        /// </summary>
        /// <param name="ex">��������� ����������</param>
        /// <param name="policyName">��� �������� ���������</param>
        /// <returns>������ - ���� ������ ���������� � �� ��������� �� �������</returns>
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

        /// <summary> ������� � ������������� ������ </summary>
        public event EventHandler<ExceptionEventArgs> ExceptionOccure;

        private void OnExceptionOccure(ExceptionEventArgs e)
        {
            var handler = ExceptionOccure;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// �������������� ���������� � ������. ������������� ����� ������ ��������� ������ ������ ������
        /// </summary>
        /// <param name="ex">����������</param>
        /// <returns>��������� �������� ����������</returns>
        [Obsolete("Use ExceptionHelper.ExceptionToString instead")]
        public static string ExceptionToString(Exception ex)
        {
            //TODO: �������� ��� ������ ����� �� ExceptionHelper
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