namespace External.Api;

public class Workflow(string workflowId)
{
    public string WorkflowId { get; set; } = workflowId;

    public Status WorkflowStatus { get; set; } = Status.Initializing;

    public DateTime WorkflowStartTime { get; set; } = DateTime.UtcNow;

    public DateTime? WorkflowEndTime { get; set; }

    public bool IsComplete { get { return WorkflowEndTime.HasValue; } }

    public string WorkflowErrorMessage { get; set; }

    public IDictionary<EntityType, WorkflowEntity> WorkflowEntities { get; set; } = new Dictionary<EntityType, WorkflowEntity>();
}