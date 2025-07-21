using System.Text.Json;
using StackExchange.Redis;

namespace ScanInvoker.Jobs;

public class InvokeProjectScanner : BackgroundService
{
    private readonly ILogger<InvokeProjectScanner> _logger;
    private readonly IDatabase _database;
    private readonly IHostEnvironment _env;
    
    public InvokeProjectScanner(ILogger<InvokeProjectScanner> logger,
        Func<string, IConnectionMultiplexer> connectionFactory,
        IHostEnvironment env)
    {
        _logger = logger;
        var connectionMultiplexer = connectionFactory("queue");
        _database = connectionMultiplexer.GetDatabase();
        _env = env;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var jsonPayload = await _database.ListGetByIndexAsync("analyzer_queue", 0);

            try
            {
                if (!jsonPayload.HasValue)
                {
                    //Waiting a message in the queue 
                    await Task.Delay(1000, stoppingToken);
                    continue;
                }

                var payload = JsonSerializer.Deserialize<JsonElement>(jsonPayload);
                
                var relativePath = payload.GetProperty("path").GetString();
                var absolutePath = Path.Combine(_env.ContentRootPath, relativePath);

                _logger.LogInformation(File.Exists(absolutePath)
                    ? "Successfully invoke project scanner"
                    : "No project scanner found");
                
                await _database.ListRemoveAsync("analyzer_queue", jsonPayload, count: 1);
            }
            catch (Exception ex)
            {
                await _database.ListRemoveAsync("analyzer_queue", jsonPayload, count: 1);
                _logger.LogError(ex, "Failed to invoke project scanner");
            }
        }
    }
}