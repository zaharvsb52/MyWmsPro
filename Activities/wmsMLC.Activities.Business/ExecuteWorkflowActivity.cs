using System;
using System.Activities;
using System.Activities.Tracking;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Activities.Business
{
    public class ExecuteWorkflowActivity : NativeActivity
    {
        private readonly Dictionary<string, string> _wfCache = new Dictionary<string, string>();
        private bool _canInduceIdle;
        private Bookmark _bookmark;
        private Guid? _parentWwfinstanceId;

        public ExecuteWorkflowActivity()
        {
            DisplayName = "Выполнение Workflow";
        }

        #region . Properties .
        [Required]
        [DisplayName(@"Код Workflow")]
        public InArgument<string> WorkflowCode { get; set; }

        [DisplayName(@"Контекст")]
        [Description(@"Контекст, который будет передан в вызываемый Workflow")]
        public InOutArgument<BpContext> InnerBpContext { get; set; }

        // indicate to the runtime that this activity can go idle
        protected override bool CanInduceIdle
        {
            get
            {
                return _canInduceIdle || base.CanInduceIdle;
            }
        }
        #endregion . Properties .

        #region . Methods .
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, WorkflowCode, type.ExtractPropertyName(() => WorkflowCode));
            ActivityHelpers.AddCacheMetadata(collection, metadata, InnerBpContext, type.ExtractPropertyName(() => InnerBpContext));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var ext = ActivityHelpers.GetTraceExtension(context);
            var workflowXaml = GetWorkflowXaml(context);
            var bpContext = InnerBpContext.Get(context);

            var parentWwfinstanceId = ext == null
                ? null
                : ext.Get<Guid?>(TraceExtension.ParentWfInstanceIdPropertyName);

            if (parentWwfinstanceId.HasValue && bpContext != null)
            {
                _parentWwfinstanceId = parentWwfinstanceId;
                _canInduceIdle = true;
                _bookmark = context.CreateBookmark("ExecuteWorkflowActivityBookmark_" + Guid.NewGuid(), OnBookmarkResume);

                var executionContext = new ExecutionContext(string.Empty,
                    new Dictionary<string, object>
                {
                    {BpContext.BpContextArgumentName, bpContext}
                });
                var engine = IoC.Instance.Resolve<IProcessExecutorEngine>(WorkflowProcessExecutorConstants.Workflow);
                engine.Run(workflowXaml: workflowXaml, context: executionContext, completedHandler: ctx =>
                {
                    ctx.ParentInstanceId = _parentWwfinstanceId;
                    ctx.ShouldResumeBookmark = _bookmark.Name;
                });
            }
            else
            {
                DynamicActivity activity;
                using (var reader = new StringReader(workflowXaml))
                    activity = (DynamicActivity) ActivityXamlServices.Load(reader);

                if (bpContext == null)
                {
                    WorkflowInvoker.Invoke(activity);
                }
                else
                {
                    var inputs = new Dictionary<string, object> { { BpContext.BpContextArgumentName, bpContext } };
                    WorkflowInvoker.Invoke(activity, inputs);
                    InnerBpContext.Set(context, bpContext);
                }
            }
        }

        private void OnBookmarkResume(NativeActivityContext context, Bookmark bookmark, object value)
        {
            _canInduceIdle = false;
            _parentWwfinstanceId = null;
            _bookmark = null;

            var completeContext = value as CompleteContext;
            if (completeContext == null)
                return;

            //Поднимаем ошибку
            if (completeContext.Exception != null)
                throw completeContext.Exception;

            var parameters = completeContext.Parameters;
            if (parameters != null && parameters.ContainsKey(BpContext.BpContextArgumentName))
            {
                var bpcontext = (BpContext) parameters[BpContext.BpContextArgumentName];
                InnerBpContext.Set(context, bpcontext);
            }
        }

        private string GetWorkflowXaml(NativeActivityContext context)
        {
            var workflowCode = WorkflowCode.Get(context);

            if (ActivityHelpers.UseActivityStackTrace(context))
            {
                var tracking = new CustomTrackingRecord(DisplayName);
                tracking.Data.Add("WorkflowCode", string.Format("Workflow='{0}'", workflowCode));
                context.Track(tracking);
            }

            if (string.IsNullOrEmpty(workflowCode))
                throw new OperationException("WorkflowCode is null.");

            if (_wfCache.ContainsKey(workflowCode))
                return _wfCache[workflowCode];

            _wfCache[workflowCode] = ActivityHelpers.GetWorkflowXaml(workflowCode);
            return _wfCache[workflowCode];
        }

        #endregion . Methods .
    }
}
