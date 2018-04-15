using System.Collections.Generic;

namespace wmsMLC.Business.Objects.Processes
{
    public class ExecutionContext
    {
        public ExecutionContext(BPProcess bpProcess, Dictionary<string, object> parameters)
        {
            BpProcess = bpProcess;
            Parameters = parameters;
        }

        public ExecutionContext(string workflowCode, Dictionary<string, object> parameters)
        {
            WorkflowCode = workflowCode;
            Parameters = parameters;
        }

        public BPProcess BpProcess { get; private set; }
        public string WorkflowCode { get; private set; }
        public Dictionary<string, object> Parameters { get; set; }
    }
}