using System.Text.Json;
using Akka.Actor;
using Common.Messaging.Redis;
using Tracing.Worker.Services;
using StackExchange.Redis;

namespace Tracing.Worker.BackgroundServices;

public abstract class EntityMonitorServiceBase<TSupervisor, TMonitor, TStartedMessage> : BackgroundService
    where TSupervisor : ActorBase
{
    protected abstract string ActorCollectionName { get; }
    protected abstract string MessageType { get; }

    private ActorSystemService _actorSystem;
    private IActorRef _supervisor;
    private readonly ConnectionMultiplexer _redis;
    private ISubscriber _subscriber;
    private readonly IConfiguration _config;
    private readonly ILogger _logger;

    public EntityMonitorServiceBase(RedisClient redisClient, ActorSystemService actorSystem, IConfiguration config, ILogger logger)
    {
        _actorSystem = actorSystem;
        _redis = redisClient.ConnectionMultiplexer;
        _config = config;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var name = GetName();
        var enabled = _config.GetValue<bool>($"TracingSample:{name}:Enabled");
        if (enabled)
        {
            _supervisor = _actorSystem.ActorOf(Props.Create<TSupervisor>(), ActorCollectionName);

            _subscriber = _redis.GetSubscriber();
            _subscriber.Subscribe(MessageType, (channel, value) =>
            {
                _logger.LogDebug($"Received message on channel: {channel}");
                _logger.LogTrace(value);
                var message = JsonSerializer.Deserialize<TStartedMessage>(value);
                _supervisor.Tell(message);
            });
            _logger.LogInformation($"Listening on channel: {MessageType}");
        }
        else
        {
            _logger.LogInformation($"Monitor: {name} not enabled in config; will not collect metrics");
        }

        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        if (_subscriber != null)
        {
            _logger.LogInformation("Unsubscribing from Redis");
            _subscriber.Unsubscribe(MessageType);
        }
        return Task.CompletedTask;
    }

    private static string GetName()
    {
        return typeof(TMonitor).Name;
    }
}