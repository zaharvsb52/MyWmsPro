using System;
using System.Collections.Generic;

namespace wmsMLC.Business.Managers.Processes
{
    public interface IProcessHost
    {
        /// <summary>
        /// Запускает или восстанавливает работу процесса.
        /// </summary>
        /// <param name="instanceId">Уникальный номер процесса</param>
        /// <param name="activityXaml">Исходный код процесса (xaml)</param>
        /// <param name="inputs">Входные аргументы процесса</param>
        /// <param name="completedHandler"></param>
        void CreateAndRun(Guid instanceId, string activityXaml, IDictionary<string, object> inputs, Action<CompleteContext> completedHandler = null);

        /// <summary>
        /// Получение списка процессов.
        /// </summary>
        /// <returns>Список уникальных номеров процессов</returns>
        IEnumerable<Guid> GetInstances();

        /// <summary>
        /// Прерывает указанный процесс.
        /// </summary>
        /// <param name="instanceId">Уникальный номер процесса</param>
        /// <param name="reason">причина прекращения</param>
        void Terminate(Guid instanceId, string reason);

        /// <summary>
        /// Приостанавливает указанный процесс.
        /// </summary>
        /// <param name="instanceId">Уникальный номер процесса</param>
        /// <param name="reason">причина остановки</param>
        void Suspend(Guid instanceId, string reason);

        /// <summary>
        /// Восстанавливает указанный процесс.
        /// </summary>
        /// <param name="instanceId">Уникальный номер процесса</param>
        /// <param name="value"></param>
        void Resume(Guid instanceId, object value);
    }

    public class CompleteContext
    {
        public CompleteContext()
        {
            State = WfCompleteState.None;
        }

        public CompleteContext(Exception ex)
        {
            State = WfCompleteState.Error;
            Exception = ex;
        }

        public IDictionary<string, object> Parameters { get; protected set; }

        public WfCompleteState State { get; protected set; }

        public Exception Exception { get; protected set; }

        /// <summary>
        /// Если значение определено, то процесс будет пытаться сделать ResumeBookmark.
        /// </summary>
        public string ShouldResumeBookmark { get; set; }
        public Guid? ParentInstanceId { get; set; }
    }

    public enum WfCompleteState
    {
        None,
        InProgress,
        Terminate,
        Error,
        Success
    }
}
