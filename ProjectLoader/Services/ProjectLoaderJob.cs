using StackExchange.Redis;
using System.Text.Json;
using Serilog;

namespace ProjectLoader.Services
{
    public class ProjectLoaderJob : BackgroundService
    {
        private readonly ILogger<ProjectLoaderJob> _logger;
        private readonly IDatabase _database;
        private readonly HttpClient _httpClient;

        public ProjectLoaderJob(ILogger<ProjectLoaderJob> logger,
            Func<string, IConnectionMultiplexer> connectionFactory, HttpClient httpClient)
        {
            _logger = logger;
            var connectionMultiplexer = connectionFactory("queue");
            _database = connectionMultiplexer.GetDatabase();
            _httpClient = httpClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var jsonPayload = await _database.ListGetByIndexAsync("file_download_queue", 0);

                    if (!jsonPayload.HasValue)
                    {
                        //Waiting a message in the queue 
                        await Task.Delay(1000, stoppingToken);
                        continue;
                    }

                    var payload = JsonSerializer.Deserialize<JsonElement>(jsonPayload);

                    if (payload.ValueKind == JsonValueKind.Object)
                    {
                        var url = payload.GetProperty("Url").GetString();
                        var branch = payload.TryGetProperty("Branch", out var branchProperty)
                            ? branchProperty.GetString()
                            : null;

                        Log.Information("Task to download file: {Url}", url);

                        string sanitizedFileName = await DownloadFile(url, branch);

                        Log.Information("Download task completed for: {Url}", url);
                        
                        var analyzerPath = new { path = $"ProjectLoader/{sanitizedFileName}"};
                        
                        string analyzerJson = JsonSerializer.Serialize(analyzerPath);
                        
                        await _database.ListLeftPushAsync("analyzer_queue", analyzerJson);
                        
                        Log.Information("Added file to analyzer queue: {Payload}", analyzerJson);
                    }
                    else
                    {
                        Log.Warning("Failed to deserialize queue item.");
                    }
                    
                    await _database.ListRemoveAsync("file_download_queue", jsonPayload, count: 1);
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "Fatal error outside item processing.");
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }

        private async Task<string> DownloadFile(string url, string? branchName)
        {
            var decodedUrl = Uri.UnescapeDataString(url);
            if (!string.IsNullOrEmpty(branchName) && decodedUrl.Contains("https://github.com/"))
            {
                Log.Information("Request for downloading file from {Url} with branch {BranchName}.", decodedUrl,
                    branchName);
                HttpResponseMessage response =
                    await _httpClient.GetAsync($"{decodedUrl}/archive/refs/heads/{branchName}.zip");

                if (!response.IsSuccessStatusCode)
                {
                    Log.Error(
                        "Failed to download file from {Url} with branch {BranchName}. HTTP Status Code: {StatusCode}",
                        decodedUrl, branchName, response.StatusCode);
                    response.EnsureSuccessStatusCode();
                }

                Log.Information("Downloading file from {Url} to branch {BranchName}.", decodedUrl, branchName);

                byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();

                if (fileBytes == null || fileBytes.Length == 0)
                {
                    Log.Error("Failed to download file from {Url} to branch {BranchName}. The file is empty.",
                        decodedUrl, branchName);
                    throw new Exception("Download failed.");
                }

                var sanitizedFileName =
                    $"{decodedUrl.Replace("https://github.com/", "").Replace("/", "_").Replace(":", "_")}.zip";

                await File.WriteAllBytesAsync(sanitizedFileName, fileBytes);

                Log.Information("File from {Url} successfully downloaded and saved as {FileName}.", decodedUrl,
                    sanitizedFileName);

                return sanitizedFileName;
            }
            else
            {
                Log.Information("Request for downloading file from {Url}.", decodedUrl);
                HttpResponseMessage response = await _httpClient.GetAsync(decodedUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Log.Error("Failed to download file from {Url}. HTTP Status Code: {StatusCode}", decodedUrl,
                        response.StatusCode);
                    response.EnsureSuccessStatusCode();
                }

                Log.Information("Downloading file from {Url}.", decodedUrl);

                byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();

                var sanitizedFileName = decodedUrl.Replace("/", "_").Replace(":", "_");

                await File.WriteAllBytesAsync(sanitizedFileName, fileBytes);

                Log.Information("File from {Url} successfully downloaded and saved as {FileName}.", decodedUrl,
                    sanitizedFileName);
                
                return sanitizedFileName;
            }
        }
    }
}