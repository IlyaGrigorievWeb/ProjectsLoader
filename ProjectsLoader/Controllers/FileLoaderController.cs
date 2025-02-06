using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectsLoader.Services;

namespace ProjectsLoader.Controllers;

public static class FileLoaderControllerRoutes
{
    public const string DownloadFile = "{url}/branch/{branchName}";
}

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
    [Route(FileLoaderControllerRoutes.DownloadFile)]
    public async Task<bool> DownloadFile(string url, string? branchName = null)
    {
        return await _fileLoaderService.DownloadFile(url, branchName);
    }
}