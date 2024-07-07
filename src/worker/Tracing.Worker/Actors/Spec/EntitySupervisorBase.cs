using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Event;
using Common.Messaging.Messages;

namespace Tracing.Worker.Actors;

public abstract class EntitySupervisorBase<TMonitor, TStartedMessage, TEndedMessage> : ReceiveActor, ILogReceive
    where TMonitor : ActorBase
    where TStartedMessage : IEntityMessage
    where TEndedMessage : IEntityMessage
{
    protected ILoggingAdapter _log { get; } = Context.GetLogger();

    private readonly Dictionary<string, IActorRef> _monitors = [];

    protected override void PreStart() => _log.Info($"{GetName()} started");
    protected override void PostStop() => _log.Info($"{GetName()} stopped");

    public EntitySupervisorBase()
    {
        Receive<TStartedMessage>(StartMonitor);

        Receive<TEndedMessage>(EndMonitor);
    }

    private void StartMonitor(TStartedMessage started)
    {
        var id = started.GetId();
        _log.Info($"Creating monitor actor for: {id}");
        var props = DependencyResolver.For(Context.System).Props<TMonitor>();
        var monitor = Context.ActorOf(props, id);
        _monitors.Add(id, monitor);
        monitor.Forward(started);
    }

    private void EndMonitor(TEndedMessage ended)
    {
        var id = ended.GetId();
        if (_monitors.ContainsKey(id))
        {
            _log.Info($"Stopping monitor actor for: {id}");
            var monitor = _monitors[id];
            Context.Stop(monitor);
            _monitors.Remove(id);
        }
    }

    private string GetName()
    {
        return GetType().Name;
    }
}