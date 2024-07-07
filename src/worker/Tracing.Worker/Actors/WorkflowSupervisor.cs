using Common.Messaging.Events;
using Tracing.Worker.Messages;

namespace Tracing.Worker.Actors;

public class WorkflowSupervisor : EntitySupervisorBase<WorkflowMonitor, WorkflowStartedEvent, WorkflowEnded>
{
}