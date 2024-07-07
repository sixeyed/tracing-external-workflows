using System.Diagnostics;
using Akka.Actor;
using Akka.Event;
using External.Api.Client;
using Tracing.Worker.Messages;
using Status = External.Api.Client.Status;

namespace Tracing.Worker.Actors;

public class WorkflowEntityMonitor : ReceiveActor, ILogReceive
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly EntityType _entityType;
    private Activity _workflowActivity;
    private Activity _activity;
    private Status _lastStatus;

    public bool IsFinished { get; private set; }

    public static Props Props(EntityType entityType, Activity workflowActivity) => Akka.Actor.Props.Create(() => new WorkflowEntityMonitor(entityType, workflowActivity));

    public WorkflowEntityMonitor(EntityType entityType, Activity workflowActivity)
    {
        _entityType = entityType;
        _workflowActivity = workflowActivity;

        Receive<WorkflowUpdated>(UpdateActivity);
    }

    protected override void PreStart() => _log.Info("WorkflowEntityMonitor started");
    protected override void PostStop() => _log.Info("WorkflowEntityMonitor stopped");

    private void UpdateActivity(WorkflowUpdated updated)
    {
        _log.Info("Update received");
        if (_workflowActivity != null)
        {
            var workflow = updated.Workflow;
            var entity = workflow.GetEntity(_entityType);
            if (entity != null)
            {
                if (_activity == null)
                {
                    StartActivity(entity);
                }
                SetStatusEvent(entity);
                if (entity.IsComplete)
                {
                    StopActivity(entity);
                }
            }
        }
    }

    private void StartActivity(WorkflowEntity entity)
    {
        Activity.Current = _workflowActivity;
        _activity = Instrumentation.Tracing.ActivitySource.StartActivity(name: entity.EntityType.ToString(), kind: ActivityKind.Internal, startTime: entity.EntityStartTime);
        _log.Info($"Started activity. Is recording: {_activity != null}");
        if (_activity != null)
        {
            _activity.AddTagIfNew("type", _entityType);
            _activity.AddTagIfNew("id", entity.EntityId);
            _activity.AddTagIfNew("startTime", entity.EntityStartTime);
            _log.Debug("Set activity tags");
        }
    }

    private void SetStatusEvent(WorkflowEntity entity)
    {
        var currentStatus = entity.EntityStatus;
        if (_activity != null && currentStatus != _lastStatus)
        {
            _activity.AddEvent(new ActivityEvent(currentStatus.ToString()));
            _lastStatus = currentStatus;
            _log.Debug($"Set activity event, status: {currentStatus}");
        }
    }

    private void StopActivity(WorkflowEntity entity)
    {
        if (_activity != null)
        {
            _log.Info("Terminating");
            _activity.AddTagIfNew("endTime", entity.EntityEndTime);
            if (string.IsNullOrEmpty(entity.EntityErrorMessage))
            {
                _activity.SetStatus(ActivityStatusCode.Ok);
            }
            else
            {
                _activity.SetStatus(ActivityStatusCode.Error, entity.EntityErrorMessage);
            }
            _activity.SetEndTime(entity.EntityEndTime.Value.DateTime);
            _activity.Stop();
            _log.Debug($"Stopped activity, status: {_activity.Status}");

            var ended = new WorkflowEntityEnded(_entityType);
            Context.Parent.Tell(ended, Self);
        }
    }
}