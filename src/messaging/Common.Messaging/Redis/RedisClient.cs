using StackExchange.Redis;

namespace Common.Messaging.Redis;

public class RedisClient
{
    public ConnectionMultiplexer ConnectionMultiplexer { get; private set; }
    public IDatabase Db 
    { 
        get 
        {
            return ConnectionMultiplexer.GetDatabase();
        }
    }

    public string ConnectionString { get; private set; } 

    public RedisClient(IConfiguration config, ILogger<RedisClient> logger)
    {
        ConnectionString = config["TracingSample:Redis:ConnectionString"];
        logger.LogInformation($"Connecting to Redis at: {ConnectionString}");
        ConnectionMultiplexer = ConnectionMultiplexer.Connect(ConnectionString);
    }
}
