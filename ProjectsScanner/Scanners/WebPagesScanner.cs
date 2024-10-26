using Contracts.Entities;
using Contracts.Interfaces;
using ProjectsLoader.Models;
using System.Text.Json;

namespace ProjectsScanner.Scanners;

public class WebPagesScanner : IProjectScanner
{
    private readonly HttpClient _httpClient;

    public WebPagesScanner(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public string getProjectLanguage()
    {
        throw new NotImplementedException();
    }

    public WebFrameworks getProjectWebFramework()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get metadate by URL
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public async Task<GitHubProject> GetMetaInfoByURL(string url)
    {
        WebFrameworks webFramework = WebFrameworks.Uncertain;

        string urlPostfix = url.Replace("https://github.com/", "");
        var segments = urlPostfix.Split('/');
        var owner = segments[0];
        var repo = segments[1];

        string apiGitHubUrl = $"https://api.github.com/repos/";

        _httpClient.DefaultRequestHeaders.Add("User-Agent", "ProjectLoader");

        var responseToApi = await _httpClient.GetAsync($"{apiGitHubUrl}{owner}/{repo}");
        responseToApi.EnsureSuccessStatusCode();

        var jsonContent = await responseToApi.Content.ReadAsStringAsync();

        var repoInfo = JsonSerializer.Deserialize<GitHubApiResponse>(jsonContent);

        var responseToApiForReadme = await _httpClient.GetAsync($"{apiGitHubUrl}{owner}/{repo}/readme");
        responseToApiForReadme.EnsureSuccessStatusCode();

        var jsonReadme = await responseToApiForReadme.Content.ReadAsStringAsync();

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
}