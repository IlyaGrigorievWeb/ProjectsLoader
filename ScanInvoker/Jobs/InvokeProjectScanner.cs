using System.Text.Json;
using ScanInvoker.Interfaces;
using StackExchange.Redis;
using Storages.EntitiesStorage;

namespace ScanInvoker.Jobs;

public class InvokeProjectScanner : BackgroundService
{
    private readonly ILogger<InvokeProjectScanner> _logger;
    private readonly IDatabase _database;
    private readonly IHostEnvironment _env;
    private readonly IProjectAnalyzer _projectAnalyzer;
    private readonly IArchiveService _archiveService;
    private readonly IProjectClusteringService _projectClusteringService;
    private readonly IServiceProvider _serviceProvider;

    
    public InvokeProjectScanner(ILogger<InvokeProjectScanner> logger,
        Func<string, IConnectionMultiplexer> connectionFactory,
        IHostEnvironment env,
        IProjectAnalyzer projectAnalyzer,
        IArchiveService archiveService,
        IProjectClusteringService projectClusteringService,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        var connectionMultiplexer = connectionFactory("queue");
        _database = connectionMultiplexer.GetDatabase();
        _env = env;
        _projectAnalyzer = projectAnalyzer;
        _archiveService = archiveService;
        _projectClusteringService = projectClusteringService;
        _serviceProvider = serviceProvider;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var jsonPayload = await _database.ListGetByIndexAsync("analyzer_queue", 0);

            if (!jsonPayload.HasValue)
            {
                await Task.Delay(1000, stoppingToken);
                continue;
            }

            try
            {
                var payload = JsonSerializer.Deserialize<JsonElement>(jsonPayload);
                var absolutePath = payload.GetProperty("path").GetString();
                
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<PostgresContext>();
                
                await using (var extraction = await _archiveService.ExtractAsync(absolutePath!, stoppingToken))
                {
                    var folderPath = extraction.FolderPath;

                    if (!Directory.Exists(folderPath))
                    {
                        _logger.LogInformation("No project found");
                        throw new FileNotFoundException("No project found", folderPath);
                    }

                    var analysisResult = await Task.Run(() =>
                        _projectAnalyzer.RunAnalyzer(folderPath, stoppingToken), stoppingToken);

                    _logger.LogInformation(
                        "The project {Project} has been successfully clustered. {@Stats}",
                        folderPath,
                        analysisResult
                    );//НАВЕРНОЕ НАДО УБРАТЬ ПЕРЕД ЗАЛИВОМ

                    var projectClusteringInfo =
                        await _projectClusteringService.Calculate(analysisResult, stoppingToken);
                    
                    var projectName = Path.GetFileName(folderPath);
                    
                    projectClusteringInfo.ProjectName = projectName;
                    
                    dbContext.ProjectClusteringInfos.Add(projectClusteringInfo);
                    await dbContext.SaveChangesAsync(stoppingToken);
                    
                    _logger.LogInformation(
                        "The project math result {@Stats}",
                        projectClusteringInfo
                    );//НАВЕРНОЕ НАДО УБРАТЬ ПЕРЕД ЗАЛИВОМ
                    
                    await _database.ListRemoveAsync("analyzer_queue", jsonPayload, count: 1);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to invoke project scanner");
                await _database.ListRemoveAsync("analyzer_queue", jsonPayload, count: 1);
            }
        }
    }
}