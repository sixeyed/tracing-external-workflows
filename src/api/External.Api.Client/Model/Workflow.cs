namespace External.Api.Client;

public partial class Workflow
{
    public WorkflowEntity GetEntity(EntityType entityType)
    {
        WorkflowEntity entity = null;
        if (WorkflowEntities.ContainsKey(entityType.ToString()))
        {
            entity = WorkflowEntities[entityType.ToString()];
        }
        return entity;
    }
}