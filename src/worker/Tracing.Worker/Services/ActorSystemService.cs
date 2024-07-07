using Akka.Actor;
using Akka.Configuration;
using Akka.DependencyInjection;

namespace Tracing.Worker.Services;

public class ActorSystemService
{
    private readonly ActorSystem _actorSystem;
    protected readonly ILogger _logger;

    public IActorRef ActorOf(Props props, string name)
    {
        return _actorSystem.ActorOf(props, name);
    }

    public ActorSystemService(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<ActorSystemService> logger)
    {
        var hoconPath = configuration.GetValue<string>("TracingSample:Akka:HoconPath");
        _logger = logger;
        _logger.LogInformation($"Creating Actor Sytem with config at: {hoconPath}");
        
        var hocon = string.Empty;
        if (File.Exists(hoconPath))
        {
            hocon = File.ReadAllText(hoconPath);
        }
        else
        {
            _logger.LogWarning($"Configuration not found at: {hoconPath}; using default config");
        }

        var config = ConfigurationFactory.ParseString(hocon);
        var bootstrap = BootstrapSetup.Create().WithConfig(config);
        var di = DependencyResolverSetup.Create(serviceProvider);

        var actorSystemSetup = bootstrap.And(di);
        _actorSystem = ActorSystem.Create("worker", actorSystemSetup);
    }
}