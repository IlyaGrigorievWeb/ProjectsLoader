using Serilog;

namespace ProjectsPlanner.Services;

public class FileLoaderService
{
    private readonly GitHubService _gitHubService;
    private readonly HttpClient _httpClient;

    public FileLoaderService(HttpClient httpClient, GitHubService gitHubService)
    {
        _httpClient = httpClient;
        _gitHubService = gitHubService;
    }

    /// <summary>
    /// DownloadFile from GitHub or other source
    /// </summary>
    /// <param name="url"></param>
    /// <param name="branchName"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<bool> DownloadFile(string url, string? branchName = null)
    {
        var decodedUrl = Uri.UnescapeDataString(url);
        if (!string.IsNullOrEmpty(branchName) && decodedUrl.Contains("https://github.com/"))
        {
            Log.Information("Request for downloading file from {Url} with branch {BranchName}.", decodedUrl, branchName);
            HttpResponseMessage response = await _httpClient.GetAsync($"{decodedUrl}/archive/refs/heads/{branchName}.zip");

            if (!response.IsSuccessStatusCode)
            {
                Log.Error("Failed to download file from {Url} with branch {BranchName}. HTTP Status Code: {StatusCode}", decodedUrl, branchName, response.StatusCode);
                response.EnsureSuccessStatusCode();
            }
            
            Log.Information("Downloading file from {Url} to branch {BranchName}.", decodedUrl, branchName);

            var IsSavedMetaInfo = await _gitHubService.SaveMetaInfoByURL(decodedUrl);
            
            if (!IsSavedMetaInfo)
            {
                Log.Error("Failed to save meta info for {Url} to branch {BranchName}.", decodedUrl, branchName);
            }
            
            byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
            
            if (fileBytes == null || fileBytes.Length == 0)
            {
                Log.Error("Failed to download file from {Url} to branch {BranchName}. The file is empty.", decodedUrl, branchName);
                throw new Exception("Download failed.");
            }
            
            var sanitizedFileName = $"{decodedUrl.Replace("https://github.com/", "").Replace("/", "_").Replace(":", "_")}.zip";
            await File.WriteAllBytesAsync(sanitizedFileName, fileBytes);
            Log.Information("File from {Url} successfully downloaded and saved as {FileName}.", decodedUrl, sanitizedFileName);

            return true;
        }
        else
        {
            Log.Information("Request for downloading file from {Url}.", decodedUrl);
            HttpResponseMessage response = await _httpClient.GetAsync(decodedUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                Log.Error("Failed to download file from {Url}. HTTP Status Code: {StatusCode}", decodedUrl, response.StatusCode);
                response.EnsureSuccessStatusCode();
            }
            
            Log.Information("Downloading file from {Url}.", decodedUrl);
            
            byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
            
            var sanitizedFileName = decodedUrl.Replace("/", "_").Replace(":", "_");
            await File.WriteAllBytesAsync(sanitizedFileName, fileBytes);
            Log.Information("File from {Url} successfully downloaded and saved as {FileName}.", decodedUrl, sanitizedFileName);
            
            return true;
        }
    }

}