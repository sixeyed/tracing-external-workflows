using System.Diagnostics;
using Akka.Actor;
using Akka.Event;
using Akka.Util.Internal;
using Common.Messaging.Events;
using Tracing.Worker.Messages;
using External.Api.Client.Services;
using External.Api.Client;

namespace Tracing.Worker.Actors;

public class WorkflowMonitor : EntityMonitorBase<WorkflowStartedEvent, WorkflowUpdated, WorkflowEnded>
{
    protected override string ActivityName => "Workflow";
    private bool _workflowFinished;
    private string _workflowErrorMessage;
    private readonly Dictionary<EntityType, IActorRef> _entityMonitors = [];

    public WorkflowMonitor(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Receive<WorkflowEntityEnded>(EndEntityMonitor);
    }

    protected override void StartActivity(WorkflowStartedEvent started)
    {
        base.StartActivity(started);
    }

    protected override void SetStartedTags(WorkflowStartedEvent started)
    {
        Activity.AddTagIfNew("workflowId", started.WorkflowId);
        Activity.AddEvent(new ActivityEvent("Submitted", new DateTimeOffset(started.SubmittedAt)));
    }

    protected override void SetUpdatedTags(WorkflowUpdated updated)
    {
        var workflow = updated.Workflow;
        Activity.AddTagIfNew("startTime", workflow.WorkflowStartTime)
                .AddTagIfNew("endTime", workflow.WorkflowEndTime);
    }

    protected override WorkflowEnded GetEndedMessage()
    {
        return new WorkflowEnded(EntityId);
    }

    protected override void EndActivity(WorkflowUpdated updated)
    {
        _log.Info("Workflow ended");
        _workflowFinished = true;
        var workflow = updated.Workflow;
        Activity.SetEndTime(workflow.WorkflowEndTime.Value.DateTime);

        _workflowErrorMessage = workflow.WorkflowErrorMessage;

        var entityMonitorsStillRunning = _entityMonitors.Where(x => x.Value != null);
        if (entityMonitorsStillRunning.Any())
        {
            _log.Debug($"Workflow finished; waiting for entity monitors: {string.Join(';', entityMonitorsStillRunning.Select(x => x.Key))}");
        }
        else
        {
            Terminate(workflow.WorkflowErrorMessage);
        }
    }

    protected override async Task<WorkflowUpdated> GetUpdatedMessage()
    {
        WorkflowUpdated updated = null;
        Workflow workflow = null;
        using (var scope = _serviceProvider.CreateScope())
        {
            var workflowService = scope.ServiceProvider.GetRequiredService<WorkflowService>();
            workflow = await workflowService.GetWorkflow(EntityId);
        }
        _log.Info("Loaded workflow");

        if (workflow != null)
        {
            foreach (var entity in workflow.WorkflowEntities)
            {
                var entityType = Enum.Parse<EntityType>(entity.Key);
                if (!_entityMonitors.ContainsKey(entityType))
                {
                    var entityMonitor = Context.ActorOf(WorkflowEntityMonitor.Props(entityType, Activity), entity.Key);
                    _entityMonitors.Add(entityType, entityMonitor);
                }
            }
            updated = new WorkflowUpdated(EntityId, workflow);
            _entityMonitors.Values.Where(x => x != null).ForEach(x => x.Tell(updated, Self));
        }
        return updated;
    }

    private void EndEntityMonitor(WorkflowEntityEnded ended)
    {
        var key = ended.EntityKey;
        if (_entityMonitors.ContainsKey(key))
        {
            _log.Info($"Stopping WorkflowEntityMonitor actor for: {key}");
            var monitor = _entityMonitors[key];
            Context.Stop(monitor);
            _entityMonitors[key] = null;
        }

        var entityMonitorsStillRunning = _entityMonitors.Where(x => x.Value != null);
        if (_workflowFinished && !entityMonitorsStillRunning.Any())
        {
            _log.Debug("Workflow finished; all entity monitors finished");
            Terminate(_workflowErrorMessage);
        }
    }
}