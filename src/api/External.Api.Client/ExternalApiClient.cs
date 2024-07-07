using Microsoft.Extensions.DependencyInjection;

namespace External.Api.Client;

public partial class ExternalApiClient
{
    private ILogger _logger;

    [ActivatorUtilitiesConstructor]
    public ExternalApiClient(IConfiguration config, HttpClient httpClient, ILogger<ExternalApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = new System.Lazy<Newtonsoft.Json.JsonSerializerSettings>(CreateSerializerSettings);
        BaseUrl = config["TracingSample:ExternalApi:BaseUrl"];
        _logger.LogInformation($"API client using base URL: {BaseUrl}");
    }
}