using System.Text.Json;
using ScanInvoker.Interfaces;
using StackExchange.Redis;

namespace ScanInvoker.Jobs;

public class InvokeProjectScanner : BackgroundService
{
    private readonly ILogger<InvokeProjectScanner> _logger;
    private readonly IDatabase _database;
    private readonly IHostEnvironment _env;
    private readonly IProjectAnalyzer _projectAnalyzer;
    private readonly IArchiveService _archiveService;

    
    public InvokeProjectScanner(ILogger<InvokeProjectScanner> logger,
        Func<string, IConnectionMultiplexer> connectionFactory,
        IHostEnvironment env,
        IProjectAnalyzer projectAnalyzer,
        IArchiveService archiveService)
    {
        _logger = logger;
        var connectionMultiplexer = connectionFactory("queue");
        _database = connectionMultiplexer.GetDatabase();
        _env = env;
        _projectAnalyzer = projectAnalyzer;
        _archiveService = archiveService;
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
                
                var absolutePath = payload.GetProperty("path").GetString();
                
                var folderPath = await _archiveService.ExtractAsync(absolutePath, stoppingToken);

                if (!Directory.Exists(folderPath))
                {
                    _logger.LogInformation("No project found");
                    throw new FileNotFoundException("No project found", folderPath);
                }
                
                var analysisResult = await Task.Run(() =>
                    _projectAnalyzer.RunAnalyzer(folderPath, stoppingToken), stoppingToken);
                
                _logger.LogInformation("The project has been successfully clustered");
                
                Directory.Delete(folderPath, true);
                
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