using Contracts.Entities;
using Contracts.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectsLoader.Services;
using ProjectsScanner.Scanners;

namespace ProjectsLoader.Controllers;

public static class ProjectsControllerRoutes
{
    public const string GetGitHubProject = "{id:guid}";
    public const string GetAllGitHubProject = "{framework:int}";
    public const string GetMetaInfoByURL = "{url}";
    public const string SaveMetaInfoByURL = "{url}";
    public const string SaveMetaInfo = "";
    public const string UpdateMetaInfoByURL = "";
    public const string DeleteMetaInfo = "{id:guid}";
}

[Authorize]
[ApiController]
[Route("[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly GitHubService _gitHubService;

    public ProjectsController(GitHubService gitHubService)
    {
        _gitHubService = gitHubService;
    }

    /// <summary>
    /// Get GitHubProject by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    [Route(ProjectsControllerRoutes.GetGitHubProject)]
    public Task<IQueryable<GitHubProject>> GetGitHubProject(Guid id)
    {
        return Task.FromResult(_gitHubService.Get().Where(x => x.Id == id));
    }

    /// <summary>
    /// Get all GitHubProject by framework
    /// </summary>
    /// <param name="framework"></param>
    /// <returns></returns>
    [HttpGet]
    [Route(ProjectsControllerRoutes.GetAllGitHubProject)]
    public async Task<IQueryable<GitHubProject>> GetAllGitHubProject(WebFrameworks framework)
    {
        return  await Task.FromResult(_gitHubService.Get().Where(x => x.WebFramework == framework));
    }

    /// <summary>
    /// Get metadata by URL
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    [HttpGet]
    [Route(ProjectsControllerRoutes.GetMetaInfoByURL)]
    public async Task<GitHubProject> GetMetaInfoByURL(string url)
    {
        return await _gitHubService.GetGitHubProject(url);
    }

    /// <summary>
    /// Saving metadata by URL
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    [HttpPost]
    [Route(ProjectsControllerRoutes.SaveMetaInfoByURL)]
    public async Task<bool> SaveMetaInfoByURL(string url)
    {
        return await _gitHubService.SaveMetaInfoByURL(url);
    }

    /// <summary>
    /// Saving metadata
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    [HttpPost]
    [Route(ProjectsControllerRoutes.SaveMetaInfo)]
    public async Task<bool> SaveMetaInfo(GitHubProject gitHubProject)
    {
        return await _gitHubService.SaveMetaInfo(gitHubProject);
    }

    /// <summary>
    /// Update metadata by URL
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    [HttpPut]
    [Route(ProjectsControllerRoutes.UpdateMetaInfoByURL)]
    public async Task<bool> UpdateMetaInfoByURL(string url)
    {
        return await _gitHubService.UpdateMetaInfoByURL(url);
    }

    /// <summary>
    /// Delete metadata
    /// </summary>
    /// <param name="id"></param>
    /// <param name="isLoadedFromDisk"></param>
    /// <returns></returns>
    [HttpDelete]
    [Route(ProjectsControllerRoutes.DeleteMetaInfo)]
    public async Task<bool> DeleteMetaInfo(Guid id, bool isLoadedFromDisk)
    {
        return await _gitHubService.DeleteMetaInfo(id, isLoadedFromDisk);
    }
}