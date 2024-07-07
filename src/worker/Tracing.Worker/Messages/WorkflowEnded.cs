using Common.Messaging.Messages;

namespace Tracing.Worker.Messages;

public class WorkflowEnded(string workflowId) : IEntityMessage
{
    public string WorkflowId { get; private set; } = workflowId;

    public string GetId() => WorkflowId;
}