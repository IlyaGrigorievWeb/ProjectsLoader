using Contracts.Entities;
using Contracts.Interfaces;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Storages.EntitiesStorage;
using System.Globalization;

namespace ProjectsLoader.Services;

public class GitHubService
{

    private readonly PostgresContext _context;
    private readonly HttpClient _httpClient;

    public GitHubService(PostgresContext context)
    {
        _context = context;
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("https://github.com/");
    }

    /// <summary>
    /// Get GitHubProject by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<GitHubProject> GetGitHubProject(Guid id)
    {
        return await _context.GitHubProjects.Where(x => x.Id == id).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Get all GitHubProject by filter
    /// </summary>
    /// <param name="framework"></param>
    /// <returns></returns>
    public async Task<List<GitHubProject>> GetAllGitHubProject(WebFrameworks framework)
    {
        return await _context.GitHubProjects.Where(x => x.WebFramework == framework).ToListAsync();
    }

    /// <summary>
    /// Get metadate by URL
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public async Task<GitHubProject> GetMetaInfoByURL(string url)
    {
        string urlPostfix = url.Replace("https://github.com/", "");

        var response = await _httpClient.GetAsync(urlPostfix);
        response.EnsureSuccessStatusCode();
        var pageContent = await response.Content.ReadAsStringAsync();

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(pageContent);

        var starsNode = htmlDoc.DocumentNode.SelectSingleNode("//a[contains(@href, '/stargazers')]/strong");
        var starsText = starsNode?.InnerText.Trim();
        int stars = 0;

        if (!string.IsNullOrEmpty(starsText))
        {
            if (starsText.EndsWith("k"))
            {
                starsText = starsText.Replace("k", "").Trim();
                if (double.TryParse(starsText, NumberStyles.Any, CultureInfo.InvariantCulture, out var starCount))
                {
                    stars = (int)(starCount * 1000);
                }
            }
            else
            {
                stars = int.TryParse(starsText.Replace(",", ""), out var starCount) ? starCount : 0;
            }
        }

        var creationDateNode = htmlDoc.DocumentNode.SelectSingleNode("//relative-time");
        var creationDateText = creationDateNode?.GetAttributeValue("datetime", "");
        var creationDate = DateTime.TryParse(creationDateText, out var date) ? date : DateTime.MinValue;

        WebFrameworks webFramework = default;
        var readmeNode = htmlDoc.DocumentNode.SelectSingleNode("//article[contains(@class, 'markdown-body')]");
        var readmeText = readmeNode?.InnerText.ToLower();

        if (readmeText != null)
        {
            if (readmeText.Contains("aspnet"))
                webFramework = WebFrameworks.Aspnet;
            else if (readmeText.Contains("django"))
                webFramework = WebFrameworks.Jango;
            else if (readmeText.Contains("spring"))
                webFramework = WebFrameworks.Spring;
        }

        var metaData = new GitHubProject
        {
            Id = Guid.NewGuid(),
            UrlPostfix = urlPostfix,
            WebFramework = webFramework,
            Stars = stars,
            CreationDate = creationDate,
        };

        return metaData;
    }

    /// <summary>
    /// Saving metadata by URL
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<bool> SaveMetaInfoByURL(string url)
    {
        var metaData = await GetMetaInfoByURL(url);

        if (metaData == null)
        {
            throw new Exception("Failed to get metadata");
        }

        _context.GitHubProjects.Add(metaData);

        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Saving metadata
    /// </summary>
    /// <param name="metaData"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<bool> SaveMetaInfo(GitHubProject metaData)
    {
        if (metaData == null)
        {
            throw new Exception("Failed to get metadata");
        }

        _context.GitHubProjects.Add(metaData);

        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Update metadata by URL
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<bool> UpdateMetaInfoByURL(string url)
    {
        var newMetaData = await GetMetaInfoByURL(url);

        if (newMetaData == null)
        {
            throw new Exception("Failed to get metadata");
        }

        var existingMetaData = _context.GitHubProjects
            .Where(x => x.Id == newMetaData.Id)
            .FirstOrDefault();

        bool hasChanges = false;

        if (existingMetaData.WebFramework != newMetaData.WebFramework)
        {
            existingMetaData.WebFramework = newMetaData.WebFramework;
            hasChanges = true;
        }

        if (existingMetaData.Stars != newMetaData.Stars)
        {
            existingMetaData.Stars = newMetaData.Stars;
            hasChanges = true;
        }

        if (existingMetaData.CreationDate != newMetaData.CreationDate)
        {
            existingMetaData.CreationDate = newMetaData.CreationDate;
            hasChanges = true;
        }

        if (hasChanges)
        {
            await _context.SaveChangesAsync();
        }

        return hasChanges;
    }

    /// <summary>
    /// Delete metadate
    /// </summary>
    /// <param name="id"></param>
    /// <param name="isLoadedFromDisk"></param>
    /// <returns></returns>
    public async Task<bool> DeleteMetaInfo(Guid id, bool isLoadedFromDisk)
    {
        if (!isLoadedFromDisk)
        {
            return false;
        }

        var existingMetaData = _context.GitHubProjects
            .Where(x => x.Id == id)
            .FirstOrDefault();

        if (existingMetaData == null)
        {
            return false;
        }

        _context.GitHubProjects.Remove(existingMetaData);

        await _context.SaveChangesAsync();

        return true;
    }

}