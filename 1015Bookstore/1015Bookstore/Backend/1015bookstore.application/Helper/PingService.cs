using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class PingService : BackgroundService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PingService> _logger;

    public PingService(IHttpClientFactory httpClientFactory, ILogger<PingService> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var response = await _httpClient.GetAsync("https://localhost:7139/api/serverping", stoppingToken);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Server is alive. Response time: {Time}ms", response.Headers.Date?.ToUniversalTime());
                }
                else
                {
                    _logger.LogError("Server is dead. Status code: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Server is dead. Error occurred while pinging the server.");
            }

            // Wait for 10 seconds before sending the next request
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    public override void Dispose()
    {
        _httpClient.Dispose();
        base.Dispose();
    }
}
