using Contracts.Entities;
using Contracts.Interfaces;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using ProjectsScanner.Scanners;
using Storages.EntitiesStorage;
using System.Globalization;

namespace ProjectsLoader.Services;

public class GitHubService
{

    private readonly PostgresContext _context;
    private readonly WebPagesScanner _webPagesScanner;

    public GitHubService(PostgresContext context, WebPagesScanner webPagesScanner)
    {
        _context = context;
        _webPagesScanner = webPagesScanner;
    }

    public async Task<GitHubProject> GetProject(Guid id)
    {
        return await _context.GitHubProjects.Where(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IList<GitHubProject>>GetAll(WebFrameworks webFramework)
    {
        return await _context.GitHubProjects.Where(x => x.WebFramework == webFramework).ToListAsync();
    }

    public async Task<GitHubProject> GetGitHubProject(string url)
    {
        return await _webPagesScanner.GetGitHubProject(url);
    }

    /// <summary>
    /// Saving metadata by URL
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<bool> SaveMetaInfoByURL(string url)
    {
        var metaData = await _webPagesScanner.GetGitHubProject(url);

        if (metaData == null)
        {
            throw new Exception("Failed to get metadata");
        }

        if (url.Contains(metaData.UrlPostfix))
        {
            return await UpdateMetaInfoByURL(url);
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
        var newMetaData = await _webPagesScanner.GetGitHubProject(url);

        if (newMetaData == null)
        {
            throw new Exception("Failed to get metadata");
        }

        var existingMetaData = _context.GitHubProjects
            .FirstOrDefault(x => x.UrlPostfix.Contains(newMetaData.UrlPostfix));

        if (existingMetaData == null)
        {
            throw new Exception("Not found project in DataBase");
        }

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