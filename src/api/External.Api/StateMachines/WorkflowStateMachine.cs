
namespace External.Api;

public class WorkflowStateMachine(WorkflowEntityStateMachine entityStateMachine, ILogger<WorkflowStateMachine> logger)
{
    public void Advance(Workflow workflow)
    {
        if (!workflow.IsComplete)
        {
            if (workflow.WorkflowEntities.Any())
            {
                var current = workflow.WorkflowEntities.Last().Value;
                entityStateMachine.Advance(current);

                if (current.IsComplete)
                {
                    if (current.EntityStatus == Status.Failed)
                    {
                        workflow.WorkflowEndTime = DateTime.UtcNow;
                        workflow.WorkflowStatus = Status.Failed;
                        workflow.WorkflowErrorMessage = $"Entity failed: {current.EntityType}";
                        logger.LogDebug($"Workflow: {workflow.WorkflowId} failed");
                    }
                    else if (current.EntityType < EntityType.OutputGenerator)
                    {
                        AddEntity(workflow, current.EntityType+1);
                    }
                    else
                    {
                        workflow.WorkflowEndTime = DateTime.UtcNow;
                        workflow.WorkflowStatus = Status.Completed;
                        logger.LogDebug($"Workflow: {workflow.WorkflowId} completed");
                    }
                }
            }
            else
            {
                AddEntity(workflow, EntityType.DataLoader);
                workflow.WorkflowStatus = Status.Processing;
            }
        }
    }

    private void AddEntity(Workflow workflow, EntityType entityType)
    {
        var entity = new WorkflowEntity(entityType);
        entityStateMachine.Initialise(entity);
        workflow.WorkflowEntities.Add(entityType, entity);
        logger.LogDebug($"Workflow: {workflow.WorkflowId} added new entity: {entityType}");
    }
}