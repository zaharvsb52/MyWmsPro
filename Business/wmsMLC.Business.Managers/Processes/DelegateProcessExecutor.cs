using System;
using wmsMLC.Business.Objects.Processes;

namespace wmsMLC.Business.Managers.Processes
{
    /// <summary>
    /// Делегирует выполенение процесса другому узлу (сервису).
    /// </summary>
    public class DelegateProcessExecutor : IProcessExecutor
    {
        public void Run(ExecutionContext context, Action<CompleteContext> completedHandler = null)
        {
          
        }
    }
}