
namespace External.Api;

public class WorkflowEntityStateMachine(ILogger<WorkflowEntityStateMachine> logger)
{
    public void Initialise(WorkflowEntity entity)
    {
        AddTransition(entity, Status.Processing);
        if (Random.Shared.Next(100) < 90)
        {
            AddTransition(entity, Status.Completed);
        }
        else
        {
            AddTransition(entity, Status.Failed);
        }
    }

    public void Advance(WorkflowEntity entity)
    {
        if (!entity.IsComplete)
        {
            var nextStatus = entity.EntityStatus+1;
            if (!entity.Transitions.ContainsKey(nextStatus))
            {
                nextStatus++;
            }
            var durationSeconds = DateTime.UtcNow.Subtract(entity.EntityStartTime).TotalSeconds;
            if (durationSeconds >= entity.Transitions[nextStatus])
            {
                entity.EntityStatus = nextStatus;
                logger.LogDebug($"{entity.EntityType}: {entity.EntityId} transitioned to status: {entity.EntityStatus}");
                if (entity.EntityStatus > Status.Processing)
                {
                    entity.EntityEndTime = DateTime.UtcNow;
                    logger.LogDebug($"{entity.EntityType}: {entity.EntityId} finished processing");
                    if (entity.EntityStatus == Status.Failed)
                    {
                        entity.EntityErrorMessage = $"Failed. Error code: {Random.Shared.Next(100)}";
                        logger.LogDebug($"{entity.EntityType}: {entity.EntityId} errored");
                    }
                }
            }
        }
    }

    private void AddTransition(WorkflowEntity entity, Status toStatus)
    {
        var duration = Random.Shared.Next(GetMinimumDuration(entity, toStatus), GetMaximumDuration(entity, toStatus));
        entity.Transitions.Add(toStatus, duration);
        logger.LogDebug($"{entity.EntityType}: {entity.EntityId} will transition to status: {toStatus}; after: {duration}s");
    }

    private int GetMinimumDuration(WorkflowEntity entity, Status status) => entity.EntityType switch
    {
        EntityType.DataLoader => status switch
        {
            Status.Processing => 10,
            Status.Completed =>  90,
            Status.Failed => 25
        },

        EntityType.Processor => status switch
        {
            Status.Processing => 20,
            Status.Completed => 300,
            Status.Failed => 30
        },

        EntityType.OutputGenerator => status switch
        {
            Status.Processing => 5,
            Status.Completed => 60,
            Status.Failed => 20
        },
    };

    private int GetMaximumDuration(WorkflowEntity entity, Status status) => entity.EntityType switch
    {
        EntityType.DataLoader => status switch
        {
            Status.Processing => 20,
            Status.Completed => 240,
            Status.Failed => 60
        },

        EntityType.Processor => status switch
        {
            Status.Processing => 30,
            Status.Completed => 900,
            Status.Failed => 180
        },

        EntityType.OutputGenerator => status switch
        {
            Status.Processing => 10,
            Status.Completed => 120,
            Status.Failed => 60
        },
    };
}