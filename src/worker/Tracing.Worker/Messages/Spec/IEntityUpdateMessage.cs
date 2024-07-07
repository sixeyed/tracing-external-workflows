namespace Tracing.Worker.Messages;

public interface IEntityUpdateMessage
{
    DateTime GetStartTime();
    DateTime GetEndTime();
    string GetStatus();
    bool HasFinished();
    string GetErrorMessage();
}