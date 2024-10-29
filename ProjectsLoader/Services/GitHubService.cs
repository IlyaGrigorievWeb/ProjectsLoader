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
        var metaData = await _webPagesScanner.GetMetaInfoByURL(url);

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
        var newMetaData = await _webPagesScanner.GetMetaInfoByURL(url);

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