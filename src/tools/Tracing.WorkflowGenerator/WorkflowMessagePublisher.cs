using Common.Messaging.Events;
using Common.Messaging.Redis;

namespace Tracing.WorkflowGenerator;

public class WorkflowMessagePublisher(RedisMessagePublisher messagePublisher, IConfiguration config, ILogger<WorkflowMessagePublisher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var workflowCount = config.GetValue<int>("TracingSample:WorkflowGenerator:WorkflowCount");
        var batchSize = config.GetValue<int>("TracingSample:WorkflowGenerator:BatchSize");
        var batchWaitMinutes = config.GetValue<int>("TracingSample:WorkflowGenerator:BatchWaitMinutes");

        logger.LogInformation($"Publishing: {workflowCount} worflow started messages; with batch size: {batchSize}; minutes between batches: {batchWaitMinutes}");
        var batchCount = workflowCount / batchSize;
        for (int i=0; i<batchCount; i++)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation($"Cancellation requested. EXITING");
                return;
            }

            await SubmitBatch(batchSize, stoppingToken);
            var waitMinutes = Random.Shared.Next(batchWaitMinutes/2, batchWaitMinutes);
            await Task.Delay(TimeSpan.FromMinutes(waitMinutes), stoppingToken);
        }
    }

    private async Task SubmitBatch(int batchSize, CancellationToken stoppingToken)
    {
        for (int i=0; i<batchSize; i++)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation($"Cancellation requested. EXITING");
                return;
            }

            var message = new WorkflowStartedEvent
            {
                WorkflowId = Guid.NewGuid().ToString(),
                SubmittedAt = DateTime.UtcNow
            };
            await messagePublisher.Publish(message);
            var waitSeconds = Random.Shared.Next(1, 20);
            await Task.Delay(TimeSpan.FromSeconds(waitSeconds), stoppingToken);
        }
    }
}