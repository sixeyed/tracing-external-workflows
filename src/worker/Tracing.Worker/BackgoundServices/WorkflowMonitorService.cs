using Common.Messaging.Events;
using Common.Messaging.Messages;
using Common.Messaging.Redis;
using Tracing.Worker.Actors;
using Tracing.Worker.Services;

namespace Tracing.Worker.BackgroundServices;

public class WorkflowMonitorService(RedisClient redisClient, ActorSystemService actorSystem, IConfiguration config, ILogger<WorkflowMonitorService> logger) : EntityMonitorServiceBase<WorkflowSupervisor, WorkflowMonitor, WorkflowStartedEvent>(redisClient, actorSystem, config, logger)
{
    protected override string ActorCollectionName => "workflows";
    protected override string MessageType => MessageTypes.Events.WorkflowStarted;
}