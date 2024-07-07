using System.Diagnostics;
using Akka.Actor;
using Akka.Event;
using Tracing.Worker.Messages;
using Common.Messaging.Messages;

namespace Tracing.Worker.Actors;

public abstract class EntityMonitorBase<TStartedMessage, TUpdatedMessage, TEndedMessage> : ReceiveActor, IWithTimers, ILogReceive
    where TStartedMessage : IEntityMessage
    where TUpdatedMessage : IEntityUpdateMessage
    where TEndedMessage : IEntityMessage
{
    protected abstract string ActivityName { get; }
    protected abstract void SetStartedTags(TStartedMessage started);
    protected abstract void SetUpdatedTags(TUpdatedMessage updated);
    protected abstract TEndedMessage GetEndedMessage();
    protected abstract Task<TUpdatedMessage> GetUpdatedMessage();

    public string EntityId { get; private set; }
    public ITimerScheduler Timers { get; set; }
    protected readonly ILoggingAdapter _log = Context.GetLogger();
    protected Activity Activity  { get; private set; }
    private string _lastStatus;
    private bool _startTimeSet;
    protected readonly IServiceProvider _serviceProvider;

    public EntityMonitorBase(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        Receive<TStartedMessage>(StartActivity);

        ReceiveAsync<MonitorRefresh>(async refresh => await RefreshStatus());

        Receive<TUpdatedMessage>(UpdateActivity);

        Receive<MonitorTimeout>(_ => Terminate("Monitor timed out"));
    }

    protected virtual void StartActivity(TStartedMessage started)
    {
        EntityId = started.GetId();
        Activity = Instrumentation.Tracing.ActivitySource.StartActivity(ActivityName, ActivityKind.Internal);
        _log.Info($"Started activity. Is recording: {Activity != null}");

        if (Activity != null)
        {
            SetStartedTags(started);
            _log.Debug("Set activity tags");
        }
    }

    private void UpdateActivity(TUpdatedMessage updated)
    {
        _log.Info("Updating entity");
        if (Activity != null)
        {
            if (!_startTimeSet)
            {
                Activity.SetStartTime(updated.GetStartTime());
                _startTimeSet = true;
                _log.Debug("Set activity start time");
            }

            SetUpdatedTags(updated);

            var currentStatus = updated.GetStatus();
            if (currentStatus != _lastStatus)
            {
                Activity.AddEvent(new ActivityEvent(currentStatus));
                _lastStatus = currentStatus;
                _log.Debug($"Set activity event, status: {currentStatus}");
            }

            if (updated.HasFinished())
            {
                EndActivity(updated);
            }
        }
    }

    protected virtual void EndActivity(TUpdatedMessage updated)
    {
        _log.Info("Entity ended");
        Activity.SetEndTime(updated.GetEndTime());
        Terminate(updated.GetErrorMessage());
    }

    protected void Terminate(string errorMessage)
    {
        _log.Info("Terminating");

        if (Activity != null)
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                Activity.SetStatus(ActivityStatusCode.Ok);
            }
            else
            {
                Activity.SetStatus(ActivityStatusCode.Error, errorMessage);
            }
            Activity.Stop();
            _log.Debug($"Stopped activity, status: {Activity.Status}");
        }

        var ended = GetEndedMessage();
        Context.Parent.Tell(ended, Self);
    }

    private async Task RefreshStatus()
    {
        _log.Info("Refresh timer triggered");
        if (Activity != null)
        {
            var update = await GetUpdatedMessage();
            if (update != null)
            {
                Self.Tell(update);
            }
        }
    }

    protected override void PreStart()
    {
        var name = GetName();
        var config = _serviceProvider.GetRequiredService<IConfiguration>();
        var initialDelaySeconds = config.GetValue<int>($"TracingSample:{name}:InitialDelaySeconds");
        var intervalSeconds = config.GetValue<int>($"TracingSample:{name}:IntervalSeconds");
        var timeoutMinutes = config.GetValue<int>($"TracingSample:{name}:TimeoutMinutes");

        _log.Info($"Monitor: {name} starting; initialDelaySeconds: {initialDelaySeconds}; intervalSeconds: {intervalSeconds}; timeoutMinutes {timeoutMinutes}");

        Timers.StartPeriodicTimer("refresh", new MonitorRefresh(), TimeSpan.FromSeconds(initialDelaySeconds), TimeSpan.FromSeconds(intervalSeconds));
        Timers.StartSingleTimer("timeout", new MonitorTimeout(), TimeSpan.FromMinutes(timeoutMinutes));
    }

    protected override void PostStop() => _log.Info($"Monitor stopped. ID: {EntityId}");

    private string GetName()
    {
        return GetType().Name;
    }
}