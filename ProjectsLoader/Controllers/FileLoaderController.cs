using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectsLoader.Services;

namespace ProjectsLoader.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class FileLoaderController : ControllerBase
{
    private readonly FileLoaderService _fileLoaderService;

    public FileLoaderController(FileLoaderService fileLoaderService)
    {
        _fileLoaderService = fileLoaderService;
    }

    /// <summary>
    /// Download file
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("GetGitHubProject")]
    public async Task<bool> DownloadFile(string url, string? branchName = null)
    {
        return await _fileLoaderService.DownloadFile(url, branchName);
    }
}