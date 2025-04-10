using Contracts.Entities;
using Contracts.Interfaces;
using ProjectsLoader.Models;
using System.Text.Json;

namespace ProjectsScanner.Scanners;

public class WebPagesScanner : IWebPagesScanner
{
    private readonly HttpClient _httpClient;

    public WebPagesScanner(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Get metadate by URL
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public async Task<GitHubProject> GetGitHubProject(string url)
    {
        string decodedUrl = Uri.UnescapeDataString(url);
        string urlPostfix = decodedUrl.Replace("https://github.com/", "");
        var segments = urlPostfix.Split('/');
        var owner = segments[0];
        var repo = segments[1];

        string apiGitHubUrl = $"https://api.github.com/repos/";

        _httpClient.DefaultRequestHeaders.Add("User-Agent", "ProjectLoader");

        var responseToApi = await _httpClient.GetAsync($"{apiGitHubUrl}{owner}/{repo}");
        responseToApi.EnsureSuccessStatusCode();

        var jsonContent = await responseToApi.Content.ReadAsStringAsync();
        
        var responseToApiForReadme = await _httpClient.GetAsync($"{apiGitHubUrl}{owner}/{repo}/readme");
        responseToApiForReadme.EnsureSuccessStatusCode();
        
        var jsonReadme = await responseToApiForReadme.Content.ReadAsStringAsync();

        var metaData = GetInfoProject(jsonContent, jsonReadme, urlPostfix);

        return metaData;
    }

    private GitHubProject GetInfoProject(string jsonContent, string jsonReadme, string urlPostfix)
    {
        WebFrameworks webFramework = WebFrameworks.Uncertain;
        var repoInfo = JsonSerializer.Deserialize<GitHubApiResponse>(jsonContent);

        if (!string.IsNullOrEmpty(jsonReadme))
        {
            var contentLower = jsonReadme.ToLower();

            if (contentLower.Contains("aspnet"))
            {
                webFramework = WebFrameworks.Aspnet;
            }
            else if (contentLower.Contains("django") || contentLower.Contains("jango"))
            {
                webFramework = WebFrameworks.Jango;
            }
            else if (contentLower.Contains("spring"))
            {
                webFramework = WebFrameworks.Spring;
            }
        }

        var metaData = new GitHubProject
        {
            Id = Guid.NewGuid(),
            UrlPostfix = urlPostfix,
            WebFramework = webFramework,
            Stars = repoInfo.stargazers_count,
            CreationDate = repoInfo.created_at,
            DefaultBranch = repoInfo.default_branch,
        };
        
        return metaData;
    }

    public async Task<GitHubProject> GetGitHubProject(User user, string repoName)
    {
        string url = "https://github.com/" + user.Login + "/" + repoName;
        var project = await GetGitHubProject(url);
        return project;
    }
}