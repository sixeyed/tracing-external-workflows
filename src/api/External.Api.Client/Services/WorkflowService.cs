using External.Api.Client;

namespace External.Api.Client.Services;

public class WorkflowService(ExternalApiClient apiClient)
{
    public async Task<Workflow> GetWorkflow(string workflowId)
    {
        return await apiClient.WorkflowsAsync(workflowId);
    }
}