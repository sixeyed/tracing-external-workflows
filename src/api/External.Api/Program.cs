using System.Text.Json.Serialization;
using External.Api;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSingleton<WorkflowEntityStateMachine>();        
        builder.Services.AddSingleton<WorkflowStateMachine>();

        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers()
                        .AddJsonOptions(options =>
                        {
                            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                        });

        var app = builder.Build();
        app.UseSwagger();
        app.UseSwaggerUI();
        app.MapControllers();
        app.Run();
    }
}
