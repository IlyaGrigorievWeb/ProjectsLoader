using System.Text.Json;
using Serilog;
using StackExchange.Redis;

namespace ProjectsPlanner.Services;

public class FileLoaderService
{
    private readonly GitHubService _gitHubService;
    private readonly IDatabase _database;

    public FileLoaderService(GitHubService gitHubService, Func<string, IConnectionMultiplexer> connectionFactory)
    {
        _gitHubService = gitHubService;
        var connectionMultiplexer = connectionFactory("queue");
        _database = connectionMultiplexer.GetDatabase();
    }

    public async Task<bool> SetToQueue(string url, string? branchName = null)
    {
        var decodedUrl = Uri.UnescapeDataString(url);

        if (!string.IsNullOrEmpty(branchName) && decodedUrl.Contains("https://github.com/"))
        {
            await _gitHubService.SaveMetaInfoByURL(decodedUrl);
        }

        var payload = new { Url = decodedUrl, Branch = branchName };
    
        string jsonPayload = JsonSerializer.Serialize(payload);

        await _database.ListLeftPushAsync("file_download_queue", jsonPayload);
    
        Log.Information("Added file to queue: {Payload}", jsonPayload);
        return true;
    }

}