using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Workflow.Helpers;
using wmsMLC.General;

namespace wmsMLC.Business.Workflow
{
    /// <summary>
    /// Хост для workflow
    /// </summary>
    public class ProcessHost : IProcessHost
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(ProcessHost));

        private readonly Lazy<WorkflowApplicationFactory> _runtime = new Lazy<WorkflowApplicationFactory>();
        private readonly Lazy<Dictionary<Guid, WfCompleteContext>> _completeActions = new Lazy<Dictionary<Guid, WfCompleteContext>>();

        public ProcessHost()
        {
            _runtime.Value.WorkflowAborted += OnWorkflowAborted;
            _runtime.Value.WorkflowCompleted += OnWorkflowCompleted;
            _runtime.Value.WorkflowIdle += OnWorkflowIdle;
            _runtime.Value.WorkflowNotHandledException += OnWorkflowNotHandledException;
            _runtime.Value.WorkflowPersistableIdle += OnWorkflowPersistableIdle;
            _runtime.Value.WorkflowUnloaded += OnWorkflowUnloaded;
        }

        private void OnWorkflowUnloaded(object sender, WorkflowApplicationEventArgs e)
        {
            _log.DebugFormat("WorkflowUnloaded {0}", e.InstanceId);
            var ctx = GetContext(e.InstanceId);
            if (ctx != null)
                RiseCompleted(e.InstanceId, ctx);
        }

        private void OnWorkflowPersistableIdle(object sender, WorkflowApplicationIdleEventArgs e)
        {
            _log.DebugFormat("WorkflowPersistableIdle {0}", e.InstanceId);
        }

        private void OnWorkflowNotHandledException(object sender, WorkflowApplicationUnhandledExceptionEventArgs e)
        {
            var mess = e.UnhandledException == null ? null : ExceptionHelper.GetErrorMessage(e.UnhandledException);
            var source = e.ExceptionSource == null ? null : e.ExceptionSource.DisplayName;
            // здесь выводим предупреждение в лог (пока есть вся информация)
            _log.WarnFormat("Occur exception in activity {1} of process {0}.{2}{3}", e.InstanceId, source, Environment.NewLine, mess);

            // обрабатываем политикой
            if (e.UnhandledException != null)
                ExceptionPolicy.Instance.HandleException(e.UnhandledException, "BP");

            var ctx = GetContext(e.InstanceId);
            if (ctx == null)
                return;
            ctx.Error(e.UnhandledException);
            RiseCompleted(e.InstanceId, ctx);
        }

        private void OnWorkflowIdle(object sender, WorkflowApplicationIdleEventArgs e)
        {
            _log.DebugFormat("WorkflowIdle {0}", e.InstanceId);
        }

        private void OnWorkflowAborted(object sender, WorkflowApplicationAbortedEventArgs e)
        {
            _log.DebugFormat("WorkflowAborted {0} : {1}", e.InstanceId, e.Reason);
            var ctx = GetContext(e.InstanceId);
            if (ctx == null)
                return;

            ctx.Error(e.Reason);
            RiseCompleted(e.InstanceId, ctx);
        }

        private void OnWorkflowCompleted(object sender, WorkflowApplicationCompletedEventArgs e)
        {
            _log.DebugFormat("WorkflowCompleted {0} : {1}", e.InstanceId, e.CompletionState);
            var ctx = GetContext(e.InstanceId);
            if (ctx == null)
                return;
            ctx.Complete(e.Outputs);
            RiseCompleted(e.InstanceId, ctx);
        }

        public void CreateAndRun(Guid instanceId, string activityXaml, IDictionary<string, object> inputs, Action<CompleteContext> completedHandler = null)
        {
            WorkflowApplication instance = null;
            try
            {
                Activity activity;
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(activityXaml)))
                    activity = ActivityXamlServices.Load(stream);

                var notUseActivityStackTrace = ConfigurationManager.AppSettings["NotUseActivityStackTrace"].To(false);
                var usePersistAndLog = ConfigurationManager.AppSettings["UseWorkflowPersistAndLog"].To(false);

                instance = _runtime.Value.CreateWorkflow(activity: activity, inputs: inputs, instanceId: instanceId, notUseActivityStackTrace: notUseActivityStackTrace, usePersistAndLog: usePersistAndLog);
                RegisterAction(instance.Id, completedHandler, inputs);
                _log.DebugFormat("Starting workflow Id: {0}, InstanceId: {1}", instance.Id, instanceId);

                instance.Run();
            }
            catch (Exception ex)
            {
                // выкидываем событие
                if (instance != null)
                    ReleaseAction(instance.Id);

                //Если ошибка не поднялась и instance == null, вызываем completedHandler
                //Убрал instance == null, т.к. не вызывался completedHandler
                if (completedHandler != null)
                    completedHandler(new CompleteContext(ex));

                // райзим ошибку
                //if (!ExceptionPolicy.Instance.HandleException(ex, "BP"))
                    throw;
            }
        }

        public IEnumerable<Guid> GetInstances()
        {
            return _runtime.Value.GetLoadedWorkflows().Select(i => i.Id);
        }

        public void Terminate(Guid instanceId, string reason)
        {
            var instance = _runtime.Value.GetWorkflow(instanceId);
            if (instance != null)
                instance.Terminate(reason);
        }

        public void Suspend(Guid instanceId, string reason)
        {
            var instance = _runtime.Value.GetWorkflow(instanceId);
            if (instance != null)
                instance.Persist();
        }

        public void Resume(Guid instanceId, object value)
        {
            var instance = _runtime.Value.GetWorkflow(instanceId);
            
            if (instance == null) 
                throw new DeveloperException("Workflow '{0}' не найден.", instanceId);

            instance.Load(instanceId);

            if (!IsInstanceWaitingForBookmark(instanceId) && instance.GetBookmarks().Count < 1)
            {
                instance.Run();
            }
            else
                instance.ResumeBookmark(instance.GetBookmarks().First().BookmarkName, value);
        }

        private void ReleaseAction(Guid instanceId)
        {
            _runtime.Value.RemoveActivityInstance(instanceId);

            if (_completeActions.Value.ContainsKey(instanceId))
                _completeActions.Value.Remove(instanceId);
        }

        private void RegisterAction(Guid instanceId, Action<CompleteContext> completedHandler, IDictionary<string, object> parameters)
        {
            if (!_completeActions.Value.ContainsKey(instanceId))
            {
                var ctx = new WfCompleteContext();
                ctx.Start(instanceId, completedHandler, parameters);
                _completeActions.Value.Add(instanceId, ctx);
            }
            else
                throw new NotImplementedException("Обновление процесса во время выполнения пока не реализовано.");
        }

        private void RiseCompleted(Guid instanceId, WfCompleteContext context)
        {
            // очищаем
            ReleaseAction(instanceId);

            // сообщаем
            context.RiseAction();

            //Post обработка после вызова Action
            if (context.ParentInstanceId.HasValue && !string.IsNullOrEmpty(context.ShouldResumeBookmark))
            {
                //небезопасный код. Может приводить к падению приложения. только логгируем
                try
                {
                    var instance = _runtime.Value.GetWorkflow(context.ParentInstanceId.Value);
                    instance.ResumeBookmark(context.ShouldResumeBookmark, context);
                }
                catch(Exception ex)
                {
                    _log.Error(ex);
                }
            }
        }

        private WfCompleteContext GetContext(Guid instanceId)
        {
            if (!_completeActions.Value.ContainsKey(instanceId))
                return null;

            return _completeActions.Value[instanceId];
        }

        private static bool IsInstanceWaitingForBookmark(Guid instanceId)
        {
            return IoHelper.ProcessUnitExistAndIdle(instanceId);
        }
    }

    public class WfCompleteContext : CompleteContext
    {
        internal Action<CompleteContext> Action { get; private set; }

        internal void Start(Guid instanceId, Action<CompleteContext> action, IDictionary<string, object> parameters)
        {
            Action = action;
            Parameters = parameters;
            State = WfCompleteState.InProgress;
        }

        internal void Error(Exception ex = null)
        {
            State = WfCompleteState.Error;
            Exception = ex;
        }

        internal void Complete(IDictionary<string, object> parameters = null, bool isTerminated = false)
        {
            State = isTerminated ? WfCompleteState.Terminate : WfCompleteState.Success;

            // переносим парметры
            if (parameters != null)
            {
                if (Parameters == null)
                    throw new DeveloperException(
                        "Процесс вернул параметры. Но при его запуске не было указано, что они ожидаются.");

                foreach (var p in parameters)
                {
                    if (Parameters.ContainsKey(p.Key) && Parameters[p.Key] != p.Value)
                        Parameters[p.Key] = p.Value;
                }
            }
        }

        internal void RiseAction()
        {
            if (Action != null)
                Action(this);
        }
    }
}
