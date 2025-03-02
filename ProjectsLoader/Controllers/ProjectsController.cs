using Contracts.Entities;
using Contracts.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectsLoader.Services;
using ProjectsScanner.Scanners;
using Serilog;

namespace ProjectsLoader.Controllers;
[Authorize]
[ApiController]
[Route("[controller]")]
public class ProjectsController : ControllerBase
{
    private const string GetGitHubProjectRoute = "{id:guid}";
    private const string GetAllGitHubProjectRoute = "{framework:int}";
    private const string GetMetaInfoByURLRoute = "{url}";
    private const string SaveMetaInfoByURLRoute = "{url}";
    private const string SaveMetaInfoRoute = "";
    private const string UpdateMetaInfoByURLRoute = "";
    private const string DeleteMetaInfoRoute = "{id:guid}";
    
    private readonly GitHubService _gitHubService;

    public ProjectsController(GitHubService gitHubService)
    {
        _gitHubService = gitHubService;
    }

    [HttpGet]
    [Route(GetGitHubProjectRoute)]
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
    [Route(GetAllGitHubProjectRoute)]
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
    [Route(GetMetaInfoByURLRoute)]
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
    [Route(SaveMetaInfoByURLRoute)]
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
    [Route(SaveMetaInfoRoute)]
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
    [Route(UpdateMetaInfoByURLRoute)]
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
    [Route(DeleteMetaInfoRoute)]
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