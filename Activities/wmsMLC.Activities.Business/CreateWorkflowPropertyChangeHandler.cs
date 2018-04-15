using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF;

namespace wmsMLC.Activities.Business
{
    public class CreateWorkflowPropertyChangeHandler : NativeActivity<IPropertyChangeHandler>
    {
        [DisplayName(@"Обработчик событий")]
        [Description("Код Workflow для обработки события изменений параметров модели")]
        public InArgument<string> PropertyChangeWorkflow { get; set; }

        public CreateWorkflowPropertyChangeHandler()
        {
            DisplayName = @"Создать обработчик событий";
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, PropertyChangeWorkflow, type.ExtractPropertyName(() => PropertyChangeWorkflow));

            metadata.SetArgumentsCollection(collection);
        }
        protected override void Execute(NativeActivityContext context)
        {
            var workflowCode = PropertyChangeWorkflow.Get(context);
            var handler = new WorkflowPropertyChangeHandler();
            Result.Set(context, !handler.Initialize(workflowCode) ? null : handler);
        }
    }

    public class WorkflowPropertyChangeHandler : IPropertyChangeHandler
    {
        DynamicActivity _activity;

        public bool Initialize(string workflowCode)
        {
            if (string.IsNullOrEmpty(workflowCode))
                return false;
            //throw new OperationException("WorkflowCode is null.");

            string workflowXaml;
            using (var mgr = (IXamlManager<BPWorkflow>)IoC.Instance.Resolve<IBaseManager<BPWorkflow>>())
            {
                var wf = mgr.Get(workflowCode);
                if (wf == null)
                    //throw new OperationException("Workflow с кодом '{0}' не существует!", workflowCode);
                    return false;
                workflowXaml = mgr.GetXaml(workflowCode);
            }

            if (string.IsNullOrEmpty(workflowXaml))
                //throw new DeveloperException("Получили пустой workflow.");
                return false;

            using (var reader = new StringReader(workflowXaml))
                _activity = (DynamicActivity)ActivityXamlServices.Load(reader);
            return true;
        }

        public void OnPropertyChange(object sender, string propertyName)
        {
            if (_activity == null)
                return;
            var context = new BpContext() { Items = new[] { sender } };
            context.Set("PropertyName", propertyName);
            var inputs = new Dictionary<string, object> { { BpContext.BpContextArgumentName, context } };
            WorkflowInvoker.Invoke(_activity, inputs);
        }
    }
}
