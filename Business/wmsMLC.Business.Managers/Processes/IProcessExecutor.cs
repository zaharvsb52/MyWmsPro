using System;
using wmsMLC.Business.Objects.Processes;

namespace wmsMLC.Business.Managers.Processes
{
    /// <summary>
    /// Интерфейс инициации выполнения процесса.
    /// </summary>
    public interface IProcessExecutor
    {
        void Run(ExecutionContext context, Action<CompleteContext> completedHandler = null);
    }
}