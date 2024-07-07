using StackExchange.Redis;
using System.Text.Json;

namespace  Common.Messaging.Redis
{
    public class RedisMessagePublisher
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly ILogger _logger;

        public RedisMessagePublisher(RedisClient redisClient, ILogger<RedisMessagePublisher> logger)
        {
            _redis = redisClient.ConnectionMultiplexer;
            _logger = logger;
        }

        public async Task Publish<TMessage>(TMessage message) where TMessage : IMessage
        {
            await Publish(message, message.MessageType);
        }

        private async Task Publish<TMessage>(TMessage message, string channel)
            where TMessage : IMessage
        {
            _logger.LogTrace($"Publishing message id: {message.CorrelationId}, type: {typeof(TMessage).Name}, to channel: {channel}");
            var subscriber = _redis.GetSubscriber();
            var json = ToJson(message);
            await subscriber.PublishAsync(channel, json);
        }
        
        private string ToJson<TMessage>(TMessage message)
            where TMessage : IMessage
        {
            var json = JsonSerializer.Serialize<TMessage>(message);
            _logger.LogTrace($"Serialized JSON: {json}");
            return json;
        }
    }
}
