using Common.Messaging.Redis;

namespace Tracing.WorkflowGenerator;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddSingleton<RedisClient>()
                        .AddSingleton<RedisMessagePublisher>()
                        .AddHostedService<WorkflowMessagePublisher>();

        var host = builder.Build();
        await host.RunAsync();
    }
}