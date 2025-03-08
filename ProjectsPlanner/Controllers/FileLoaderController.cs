using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectsPlanner.Services;
using Serilog;

namespace ProjectsPlanner.Controllers;
[Authorize]
[ApiController]
[Route("[controller]")]
public class FileLoaderController : ControllerBase
{
    private const string DownloadFileRoute = "{url}/branch/{branchName}";
    
    private readonly FileLoaderService _fileLoaderService;

    public FileLoaderController(FileLoaderService fileLoaderService)
    {
        _fileLoaderService = fileLoaderService;
    }

    /// <summary>
    /// Adds a project to the download queue
    /// </summary>
    /// <param name="url"></param>
    /// <param name="branchName"></param>
    /// <returns></returns>
    [HttpPost]
    [Route(DownloadFileRoute)]
    public async Task<bool> SetToQueue(string url, string? branchName = null)
    {
        try
        {
            return await _fileLoaderService.SetToQueue(url, branchName);
        }
        catch (Exception e)
        {
            Log.Error(e, "Error add to queue file from URL: {Url} and branch: {BranchName}", url, branchName);
            
            return false;
        }
    }
}