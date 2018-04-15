using System;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.General;

namespace wmsMLC.Business.Managers.Processes
{
    /// <summary>
    /// Реализует механизмы запуска процессов через Workflow.
    /// </summary>
    public class WorkflowProcessExecutorEngine : IProcessExecutorEngine
    {
        public void Run(ExecutionContext context, Action<CompleteContext> completedHandler = null)
        {
            string xaml;

            if (context.BpProcess == null && string.IsNullOrEmpty(context.WorkflowCode))
                throw new OperationException("Не указан процесс или Workflow.");

            var wfCode = context.WorkflowCode;
            if (context.BpProcess != null)
            {
                wfCode = context.BpProcess.WORKFLOWCODE_R;
                if (string.IsNullOrEmpty(wfCode))
                    throw new DeveloperException("Для процесса с кодом '{0}' не указан WorkFlow.");
            }
            using (var wfMgr = IoC.Instance.Resolve<IXamlManager<BPWorkflow>>())
                xaml = wfMgr.GetXaml(wfCode, true);

            Run(workflowXaml: xaml, context: context, completedHandler: completedHandler);
        }

        public void Run(string workflowXaml, ExecutionContext context, Action<CompleteContext> completedHandler = null)
        {
            if (string.IsNullOrEmpty(workflowXaml))
                throw new DeveloperException("XAML is empty");

            var host = IoC.Instance.Resolve<IProcessHost>();
            var instanceId = Guid.NewGuid();
            host.CreateAndRun(instanceId: instanceId, activityXaml: workflowXaml, inputs: context.Parameters, completedHandler: completedHandler);
        }
    }
}