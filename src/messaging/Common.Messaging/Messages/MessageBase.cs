namespace Common.Messaging.Messages;

public abstract class MessageBase : IMessage
{    
    public Guid CorrelationId  => Guid.NewGuid(); 

    public DateTime Timestamp {get; set;}

    public abstract string MessageType {get;}

    protected MessageBase()
    {
        Timestamp = DateTime.UtcNow;
    }
}