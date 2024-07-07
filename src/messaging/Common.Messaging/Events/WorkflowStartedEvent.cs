namespace Common.Messaging.Events;

public class WorkflowStartedEvent : MessageBase, IEntityMessage
{    
    public string WorkflowId { get; set;}
    public DateTime SubmittedAt {get; set;}
    
    public string GetId() => WorkflowId;

    public override string MessageType => MessageTypes.Events.WorkflowStarted;
}