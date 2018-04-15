using System;

namespace wmsMLC.Business.Objects.Processes
{
    public class BPProcess : WMSBusinessObject
    {
        public const string ExecutorPropertyName = "PROCESSEXECUTOR";
        public const string EnginePropertyName = "PROCESSENGINE";
        public const string DisablePropertyName = "PROCESSLOCKED";
        public const string NamePropertyName = "PROCESSNAME";
        public const string DescriptionPropertyName = "PROCESSDESC";
        public const string PROCESSCODEPropertyName = "PROCESSCODE";
        public const string WORKFLOWCODE_RPropertyName = "WORKFLOWCODE_R";

        #region .  Properties  .
        public string PROCESSCODE
        {
            get { return GetProperty<string>(PROCESSCODEPropertyName); }
            set { SetProperty(PROCESSCODEPropertyName, value); }
        }

        public bool Disable
        {
            get { return GetProperty<bool>(DisablePropertyName); }
            set { SetProperty(DisablePropertyName, value); }
        }

        public string Executor
        {
            get { return GetProperty<string>(ExecutorPropertyName); }
            set { SetProperty(ExecutorPropertyName, value); }
        }

        public BPExecutor ExecutorEnum
        {
            get
            {
                var str = GetProperty<string>(ExecutorPropertyName);
                return (BPExecutor)Enum.Parse(typeof(BPExecutor), str);
            }
            set { SetProperty(ExecutorPropertyName, value.ToString()); }
        }

        public string Engine
        {
            get { return GetProperty<string>(EnginePropertyName); }
            set { SetProperty(EnginePropertyName, value); }
        }

        public BPEngine EngineEnum
        {
            get
            {
                var str = GetProperty<string>(EnginePropertyName);
                return (BPEngine)Enum.Parse(typeof(BPEngine), str);
            }
            set { SetProperty(EnginePropertyName, value.ToString()); }
        }

        public string Name
        {
            get { return GetProperty<string>(NamePropertyName); }
            set { SetProperty(NamePropertyName, value); }
        }

        public string Description
        {
            get { return GetProperty<string>(DescriptionPropertyName); }
            set { SetProperty(DescriptionPropertyName, value); }
        }

        public string WORKFLOWCODE_R
        {
            get { return GetProperty<string>(WORKFLOWCODE_RPropertyName); }
            set { SetProperty(WORKFLOWCODE_RPropertyName, value); }
        }
        #endregion
    }

    public class BPProcess2Object : WMSBusinessObject
    {
    }

    public class BPLog : WMSBusinessObject
    {
    }

    public class BPTrigger : WMSBusinessObject
    {
        #region .  Constants  .
        public const string ObjectNamePropertyName = "OBJECTNAME_R";
        public const string ProcessCodePropertyName = "PROCESSCODE_R";
        public const string TriggerActionPropertyName = "TRIGGERACTION";
        public const string TriggerModePropertyName = "TRIGGERMODE";
        //TODO: переименовать поле (неправильно написана expression)
        public const string TriggerExcpressionPropertyName = "TRIGGEREXPRESSION";
        public const string ButtonCodePropertyName = "UIBUTTONCODE_R";
        public const string TriggerEntityFilterPropertyName = "TRIGGERENTITYFILTER";
        public const string TriggerOnlyByOneItemPropertyName = "TRIGGERONLYBYONEITEM";
        #endregion

        #region .  Properties  .

        public string OBJECTNAME_R
        {
            get { return GetProperty<string>(ObjectNamePropertyName); }
            set { SetProperty(ObjectNamePropertyName, value); }
        }

        public string ProcessCode
        {
            get { return GetProperty<string>(ProcessCodePropertyName); }
            set { SetProperty(ProcessCodePropertyName, value); }
        }

        public string TriggerAction
        {
            get { return GetProperty<string>(TriggerActionPropertyName); }
            set { SetProperty(TriggerActionPropertyName, value); }
        }

        public string TriggerExcpression
        {
            get { return GetProperty<string>(TriggerExcpressionPropertyName); }
            set { SetProperty(TriggerExcpressionPropertyName, value); }
        }

        public string TriggerMode
        {
            get { return GetProperty<string>(TriggerModePropertyName); }
            set { SetProperty(TriggerModePropertyName,value); }

        }
        public string ButtonCode
        {
            get { return GetProperty<string>(ButtonCodePropertyName); }
            set { SetProperty(ButtonCodePropertyName, value); }

        }

        public bool TriggerOnlyByOneItem
        {
            get { return GetProperty<bool>(TriggerOnlyByOneItemPropertyName); }
            set { SetProperty(TriggerOnlyByOneItemPropertyName, value); }

        }

        public string TriggerEntityFilter
        {
            get { return GetProperty<string>(TriggerEntityFilterPropertyName); }
            set { SetProperty(TriggerEntityFilterPropertyName, value); }

        }
        #endregion
    }

    public class BPWorkflow : WMSBusinessObject
    {
        #region .  Constants  .
        public const string XamlPropertyName = "WORKFLOWXAML";
        public const string WorkflowVersionPropertyName = "WORKFLOWVERSION";
        public const string UserCode_RPropertyName = "USERCODE_R";
        #endregion .  Constants  .

        #region .  Properties  .
        public string WorkflowXaml
        {
            get { return GetProperty<string>(XamlPropertyName); }
            set { SetProperty(XamlPropertyName, value); }
        }
        public string WorkflowVersion
        {
            get { return GetProperty<string>(WorkflowVersionPropertyName); }
            set { SetProperty(WorkflowVersionPropertyName, value); }
        }

        public string UserCode_R
        {
            get { return GetProperty<string>(UserCode_RPropertyName); }
            set { SetProperty(UserCode_RPropertyName, value); }
        }
        #endregion .  Properties  .
    }

    public enum BPEngine
    {
        WORKFLOW, CODE
    }

    public enum BPExecutor
    {
        CLIENT, SERVICE
    }
}