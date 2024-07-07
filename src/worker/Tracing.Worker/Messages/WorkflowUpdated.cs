using External.Api.Client;

namespace Tracing.Worker.Messages;

public class WorkflowUpdated(string workflowId, Workflow workflow) : IEntityUpdateMessage
{
    public string WorkflowId { get; private set; } = workflowId;

    public Workflow Workflow { get; private set; } = workflow;

    public DateTime GetStartTime()
    {
        return Workflow.WorkflowStartTime.DateTime;
    }

    public DateTime GetEndTime()
    {
        return Workflow.WorkflowEndTime.Value.DateTime;
    }

    public string GetStatus()
    {
        return Workflow.WorkflowStatus.ToString();
    }

    public bool HasFinished()
    {
        return Workflow.IsComplete;
    }

    public string GetErrorMessage()
    {
        return Workflow.WorkflowErrorMessage;
    }
}