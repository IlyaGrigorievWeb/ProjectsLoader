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

    
    public InvokeProjectScanner(ILogger<InvokeProjectScanner> logger,
        Func<string, IConnectionMultiplexer> connectionFactory,
        IHostEnvironment env,
        IProjectAnalyzer projectAnalyzer)
    {
        _logger = logger;
        var connectionMultiplexer = connectionFactory("queue");
        _database = connectionMultiplexer.GetDatabase();
        _env = env;
        _projectAnalyzer = projectAnalyzer;
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

                string testPath = "/data/projectloader/files/shadowsocks-windows-4";
                
                _logger.LogInformation(File.Exists(absolutePath)
                    ? "Successfully invoke project scanner"
                    : "No project scanner found");
                
                var analysisResult = await Task.Run(() =>
                    _projectAnalyzer.RunAnalyzer(testPath, stoppingToken), stoppingToken);
                
                var analysisTestResult = await Task.Run(() =>
                    _projectAnalyzer.RunTestAnalyzer(testPath, stoppingToken), stoppingToken);

                if (analysisResult != null)
                {
                    _logger.LogInformation($"Function count: {analysisResult.FunctionCount} PropertyCount: {analysisResult.PropertyCount}");
                    _logger.LogInformation($"TotalClassCount: {analysisTestResult.TotalClassCount} " +
                                           $"LogsClassCount: {analysisTestResult.LogsClassCount}");
                }
                
                //await _database.ListRemoveAsync("analyzer_queue", jsonPayload, count: 1);
            }
            catch (Exception ex)
            {
                await _database.ListRemoveAsync("analyzer_queue", jsonPayload, count: 1);
                _logger.LogError(ex, "Failed to invoke project scanner");
            }
        }
    }
}