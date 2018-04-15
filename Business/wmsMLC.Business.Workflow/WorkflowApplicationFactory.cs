using System;
using System.Activities;
using System.Activities.Tracking;
using System.Collections.Generic;
using System.Threading;
using wmsMLC.Business.Workflow.InstanceStores;
using wmsMLC.Business.Workflow.PersistenceParticipants;
using wmsMLC.Business.Workflow.TrackingParticipants;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Workflow
{
    public class WorkflowApplicationFactory
    {
        #region .  Fields  .

        private const int MaxActivityStackTraceCount = 10;
        private readonly Lazy<Dictionary<Guid, WorkflowApplication>> _instances = new Lazy<Dictionary<Guid, WorkflowApplication>>();
        private readonly Lazy<Dictionary<Guid, Guid>> _keyLink = new Lazy<Dictionary<Guid, Guid>>();
        private ActivityStackTrace _trackingSource;
        #endregion .  Fields  .

        #region .  Events  .

        public event EventHandler<WorkflowApplicationAbortedEventArgs> WorkflowAborted;
        public event EventHandler<WorkflowApplicationCompletedEventArgs> WorkflowCompleted;
        public event EventHandler<WorkflowApplicationIdleEventArgs> WorkflowIdle;
        public event EventHandler<WorkflowApplicationUnhandledExceptionEventArgs> WorkflowNotHandledException;
        public event EventHandler<WorkflowApplicationIdleEventArgs> WorkflowPersistableIdle;
        public event EventHandler<WorkflowApplicationEventArgs> WorkflowUnloaded;

        #endregion

        #region .  Methods  .
        public WorkflowApplication CreateWorkflow(Activity activity, IDictionary<string, object> inputs, Guid instanceId, bool notUseActivityStackTrace, bool usePersistAndLog)
        {
            if (_instances.Value.ContainsKey(instanceId))
                throw new DeveloperException("Процесс с кодом '{0}' уже существует.", instanceId);

            var result = new WorkflowApplication(activity, inputs)
            {
                SynchronizationContext = SynchronizationContext.Current
            };
            
            //Настраиваем Activity StackTrace
            ClearTrackingSource();
            if (!notUseActivityStackTrace)
            {
                _trackingSource = new ActivityStackTrace();
                result.Extensions.Add(CreateActivityTrackingParticipant());
            }

            var traceExtension = new TraceExtension
            {
                NotUseActivityStackTrace = notUseActivityStackTrace,
                UsePersistAndLog = usePersistAndLog
            };
            traceExtension.Set(TraceExtension.ParentWfInstanceIdPropertyName, instanceId);
            traceExtension.Set(TraceExtension.ActivityTrackingSourcePropertyName, _trackingSource);
            result.Extensions.Add(traceExtension);

            if (usePersistAndLog)
            {
                result.InstanceStore = new XmlWorkflowInstanceStore(result.Id);
                //Create the persistence Participant and add it to the workflow instance
                result.Extensions.Add(new XmlPersistenceParticipant(instanceId, result.Id));
                //Add a tracking participant
                result.Extensions.Add(new SaveAllEventsToFileTrackingParticipant());
            }

            _instances.Value.Add(instanceId, result);
            _keyLink.Value.Add(result.Id, instanceId);

            // пропишем события
            SubscribeEvents(result);

            if (usePersistAndLog)
                result.Persist();

            return result;
        }

        private TrackingParticipant CreateActivityTrackingParticipant()
        {
            var trackingParticipant = new ActivityStackTraceParticipant(MaxActivityStackTraceCount)
            {
                TrackingSource = _trackingSource,
                TrackingProfile = new TrackingProfile
                {
                    Name = "ActivitTrackingProfile",
                    ActivityDefinitionId = "ProcessOrder"
                }
            };
            
            //TrackingQuery query = new WorkflowInstanceQuery
            //{
            //    States = { "*" }
            //};
            //trackingParticipant.TrackingProfile.Queries.Add(query);

            TrackingQuery query = new ActivityStateQuery
            {
                States =
                {
                    //ActivityStates.Canceled,
                    //ActivityStates.Closed,
                    ActivityStates.Faulted,
                    ActivityStates.Executing
                }
            };
            trackingParticipant.TrackingProfile.Queries.Add(query);

            query = new FaultPropagationQuery();
            trackingParticipant.TrackingProfile.Queries.Add(query);

            query = new CustomTrackingQuery
            {
                ActivityName = "*",
                Name = "*"
            };
            trackingParticipant.TrackingProfile.Queries.Add(query);

            return trackingParticipant;
        }

        private void ClearTrackingSource()
        {
            if (_trackingSource != null)
            {
                _trackingSource.Clear();
                _trackingSource = null;
            }
        }

        private void SubscribeEvents(WorkflowApplication app)
        {
            app.Aborted += Aborted;
            app.Completed += Completed;
            app.OnUnhandledException += OnUnhandledException;
            app.Unloaded += Unloaded;
            app.Idle += Idle;
            app.PersistableIdle += PersistableIdle;
        }

        private void UnSubscribeEvents(WorkflowApplication app)
        {
            ClearTrackingSource();
            app.Aborted -= Aborted;
            app.Completed -= Completed;
            app.OnUnhandledException -= OnUnhandledException;
            app.Unloaded -= Unloaded;
            app.Idle -= Idle;
            app.PersistableIdle -= PersistableIdle;
        }

        public void RemoveActivityInstance(Guid activityId)
        {
            if (!_keyLink.Value.ContainsKey(activityId))
                return;

            var instanceId = _keyLink.Value[activityId];
            _keyLink.Value.Remove(activityId);

            if (!_instances.Value.ContainsKey(instanceId))
                return;

            var app = _instances.Value[instanceId];
            UnSubscribeEvents(app);
            _instances.Value.Remove(instanceId);
        }

        public WorkflowApplication GetWorkflow(Guid instanceId)
        {
            if (!_instances.Value.ContainsKey(instanceId))
                throw new DeveloperException("Процесс с кодом '{0}' не существует.", instanceId);
            return _instances.Value[instanceId];
        }

        public WorkflowApplication GetWorkflowById(Guid workflowId)
        {
            if (!_keyLink.Value.ContainsKey(workflowId))
                throw new DeveloperException("Can't found Workflow by Id '{0}'.", workflowId);
            var instansId = _keyLink.Value[workflowId];
            return GetWorkflow(instansId);
        }

        public IEnumerable<WorkflowApplication> GetLoadedWorkflows()
        {
            return _instances.Value.Values;
        }

        #region EventHandlers
        private PersistableIdleAction PersistableIdle(WorkflowApplicationIdleEventArgs a)
        {
            var h = WorkflowPersistableIdle;
            if (h != null)
                h(this, a);
            return PersistableIdleAction.Unload;
        }

        private void Idle(WorkflowApplicationIdleEventArgs a)
        {
            var h = WorkflowIdle;
            if (h != null)
                h(this, a);
        }

        private void Unloaded(WorkflowApplicationEventArgs a)
        {
            var h = WorkflowUnloaded;
            if (h != null)
                h(this, a);

            RemoveActivityInstance(a.InstanceId);
        }

        private UnhandledExceptionAction OnUnhandledException(WorkflowApplicationUnhandledExceptionEventArgs a)
        {
            var h = WorkflowNotHandledException;
            if (h != null)
                h(this, a);

            RemoveActivityInstance(a.InstanceId);
            return UnhandledExceptionAction.Terminate;
        }

        private void Completed(WorkflowApplicationCompletedEventArgs a)
        {
            var h = WorkflowCompleted;
            if (h != null)
                h(this, a);
            RemoveActivityInstance(a.InstanceId);
        }

        private void Aborted(WorkflowApplicationAbortedEventArgs a)
        {
            var h = WorkflowAborted;
            if (h != null)
                h(this, a);
            RemoveActivityInstance(a.InstanceId);
        }
        #endregion

        #endregion
    }
}