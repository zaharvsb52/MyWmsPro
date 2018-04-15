namespace wmsMLC.Business.Managers.Processes
{
    public static class BatchcodeWorkflowCodes
    {
        public static string ExecuteWorkflowCode
        {
            get { return "WfBatchIwbPosInput"; }
        }

        public static string[] WorkflowCodes
        {
            get { return new[] {"WFBATCHMO", "WfBatchBSF", "WFBATCHBJ"}; }
        }

        public static void ClearObjectCacheExecuteWorkflow()
        {
            BPWorkflowManager.ClearObjectCache(ExecuteWorkflowCode);
        }

        public static void ClearCachableWorkflow()
        {
            foreach (var wf in WorkflowCodes)
            {
                BPWorkflowManager.ClearObjectCache(wf);
            }
        }
    }
}
