using Contracts.Entities;
using Contracts.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectsLoader.Services;
using ProjectsScanner.Scanners;
using Serilog;

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

    [HttpGet]
    [Route(ProjectsControllerRoutes.GetGitHubProject)]
    public async Task<GitHubProject> GetGitHubProject(Guid id)
    {
        try
        {
            return await _gitHubService.GetProject(id);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred while getting GitHub project with ID: {ProjectId}", id);
            throw;
        }
    }

    [HttpGet]
    [Route(ProjectsControllerRoutes.GetAllGitHubProject)]
    public async Task<IList<GitHubProject>> GetAllGitHubProject(WebFrameworks framework)
    {
        try
        {
            return await _gitHubService.GetAll(framework);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred while getting all GitHub projects for framework: {Framework}", framework);
            throw;
        }
    }

    [HttpGet]
    [Route(ProjectsControllerRoutes.GetMetaInfoByURL)]
    public async Task<GitHubProject> GetMetaInfoByURL(string url)
    {
        try
        {
            return await _gitHubService.GetGitHubProject(url);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred while getting metadata for URL: {Url}", url);
            throw;
        }
    }

    [HttpPost]
    [Route(ProjectsControllerRoutes.SaveMetaInfoByURL)]
    public async Task<bool> SaveMetaInfoByURL(string url)
    {
        try
        {
            return await _gitHubService.SaveMetaInfoByURL(url);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred while saving metadata for URL: {Url}", url);
            throw;
        }
    }

    [HttpPost]
    [Route(ProjectsControllerRoutes.SaveMetaInfo)]
    public async Task<bool> SaveMetaInfo(GitHubProject gitHubProject)
    {
        try
        {
            return await _gitHubService.SaveMetaInfo(gitHubProject);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred while saving metadata for GitHub project with ID: {ProjectId}", gitHubProject.Id);
            throw;
        }
    }

    [HttpPut]
    [Route(ProjectsControllerRoutes.UpdateMetaInfoByURL)]
    public async Task<bool> UpdateMetaInfoByURL(string url)
    {
        try
        {
            return await _gitHubService.UpdateMetaInfoByURL(url);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred while updating metadata for URL: {Url}", url);
            throw;
        }
    }

    [HttpDelete]
    [Route(ProjectsControllerRoutes.DeleteMetaInfo)]
    public async Task<bool> DeleteMetaInfo(Guid id, bool isLoadedFromDisk)
    {
        try
        {
            return await _gitHubService.DeleteMetaInfo(id, isLoadedFromDisk);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred while deleting metadata for project with ID: {ProjectId}", id);
            throw;
        }
    }
}