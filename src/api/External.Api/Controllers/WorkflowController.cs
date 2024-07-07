using Microsoft.AspNetCore.Mvc;

namespace External.Api;

[ApiController]
[Route("workflows")]
public class WorkflowController(WorkflowStateMachine workflowStateMachine) : ControllerBase
{
    private static Dictionary<string, Workflow> _Workflows = new();

    [HttpGet("{workflowId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Workflow))]
    public IActionResult GetWorkflow(string workflowId)
    {
        Workflow workflow;
        
        if (!_Workflows.TryGetValue(workflowId, out Workflow value))
        {
            workflow = new Workflow(workflowId);
        }
        else
        {
            workflow = value;
            workflowStateMachine.Advance(workflow);
        }

        _Workflows[workflowId] = workflow;
        return Ok(workflow);
    }
}