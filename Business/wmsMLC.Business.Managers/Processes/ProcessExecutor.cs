using System;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.General;

namespace wmsMLC.Business.Managers.Processes
{
    /// <summary>
    /// Непосредственно выполняет процесс
    /// </summary>
    public class ProcessExecutor : IProcessExecutor
    {
        public void Run(ExecutionContext context, Action<CompleteContext> completedHandler = null)
        {
            var engine = IoC.Instance.Resolve<IProcessExecutorEngine>(context.BpProcess.Engine);
            if (engine == null) 
                throw new DeveloperException("Engine is not registered");
            engine.Run(context: context, completedHandler: completedHandler);
        }
    }
}