
using External.Api.Client;

namespace Tracing.Worker.Messages;

public class WorkflowEntityEnded(EntityType entityType)
{
    public EntityType EntityKey { get; private set; } = entityType;
}