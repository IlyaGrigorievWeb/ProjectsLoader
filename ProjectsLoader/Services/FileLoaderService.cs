using Storages.EntitiesStorage;

namespace ProjectsLoader.Services;

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
        if (!string.IsNullOrEmpty(branchName) && url.Contains("https://github.com/"))
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"{url}/archive/refs/heads/{branchName}.zip");
            response.EnsureSuccessStatusCode();
            
            await _gitHubService.SaveMetaInfoByURL(url);
            byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
            
            if (fileBytes == null || fileBytes.Length == 0)
            {
                throw new Exception("Download failed.");
            }
            
            var sanitizedFileName = $"{url.Replace("https://github.com/", "").Replace("/", "_").Replace(":", "_")}.zip";
            await File.WriteAllBytesAsync(sanitizedFileName, fileBytes);

            return true;
        }
        else
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
            
            var sanitizedFileName = url.Replace("/", "_").Replace(":", "_");
            await File.WriteAllBytesAsync(sanitizedFileName, fileBytes);

            return true;
        }
    }

}