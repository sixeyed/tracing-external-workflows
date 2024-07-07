using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tracing.Worker.Actors;
using Tracing.Worker.BackgroundServices;
using Instrumentation = Tracing.Worker.Instrumentation;
using Tracing.Worker.Services;
using External.Api.Client;
using External.Api.Client.Services;
using Common.Messaging.Redis;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // API client:
        builder.Services.AddHttpClient<ExternalApiClient>();
        builder.Services.AddTransient<WorkflowService>();

        // messaging:
        builder.Services.AddSingleton<RedisClient>();
        builder.Services.AddSingleton<RedisMessagePublisher>();

        // monitors:
        builder.Services.AddSingleton<ActorSystemService>();
        builder.Services.AddScoped<WorkflowMonitor>();
        builder.Services.AddHostedService<WorkflowMonitorService>();
        
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                                      .AddSource(Instrumentation.Tracing.NAME)
                                      .ConfigureResource(resource =>
                                        resource.AddService(
                                        serviceName: "Tracing.Worker",
                                        serviceVersion: "1.0.0",
                                        serviceNamespace: "dev1"))
                                      .AddConsoleExporter()
                                      .AddOtlpExporter()
                                      .Build();

        var host = builder.Build();
        await host.RunAsync();
    }
}