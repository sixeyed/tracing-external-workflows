namespace Common.Messaging.Messages;

public interface IMessage
{
    Guid CorrelationId { get; }

    string MessageType { get; }

    public DateTime Timestamp { get; set; }
}