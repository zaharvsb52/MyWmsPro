using System;
using wmsMLC.Business.Objects.Processes;

namespace wmsMLC.Business.Managers.Processes
{
    /// <summary>
    /// Абстракция конкретного исполнителя процесса.
    /// </summary>
    public interface IProcessExecutorEngine
    {
        void Run(ExecutionContext context, Action<CompleteContext> completedHandler = null);
        void Run(string workflowXaml, ExecutionContext context, Action<CompleteContext> completedHandler = null);
    }
}