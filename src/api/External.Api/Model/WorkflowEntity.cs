namespace External.Api;

public partial class WorkflowEntity(EntityType type)
{
    internal Dictionary<Status, long> Transitions = [];

    public EntityType EntityType { get; set; } = type;

    public string EntityId { get; set; } = Guid.NewGuid().ToString();

    public Status EntityStatus { get; set; } = Status.Initializing;

    public string EntityErrorMessage { get; set; }

    public DateTime EntityStartTime { get; set; } = DateTime.UtcNow;

    public DateTime? EntityEndTime { get; set; }

    public bool IsComplete { get { return EntityEndTime.HasValue; } }
}