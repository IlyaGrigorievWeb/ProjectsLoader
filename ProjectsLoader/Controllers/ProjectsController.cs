﻿using Contracts.Entities;
using Contracts.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectsLoader.Services;
using ProjectsScanner.Scanners;

namespace ProjectsLoader.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly GitHubService _gitHubService;
    private readonly WebPagesScanner _webPagesScanner;

    public ProjectsController(GitHubService gitHubService, WebPagesScanner webPagesScanner)
    {
        _gitHubService = gitHubService;
        _webPagesScanner = webPagesScanner;
    }

    /// <summary>
    /// Get GitHubProject by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("GetGitHubProject")]
    public async Task<GitHubProject> GetGitHubProject(Guid id)
    {
        return await _gitHubService.GetGitHubProject(id);
    }

    /// <summary>
    /// Get all GitHubProject by filter
    /// </summary>
    /// <param name="framework"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("GetAllGitHubProject")]
    public async Task<List<GitHubProject>> GetAllGitHubProject(WebFrameworks framework)
    {
        return await _gitHubService.GetAllGitHubProject(framework);
    }

    /// <summary>
    /// Get metadata by URL
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("GetMetaInfoByURL")]
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
    [Route("SaveMetaInfoByURL")]
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
    [Route("SaveMetaInfo")]
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
    [Route("UpdateMetaInfoByURL")]
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
    [Route("UpdateMetaInfoByURL")]
    public async Task<bool> DeleteMetaInfo(Guid id, bool isLoadedFromDisk)
    {
        return await _gitHubService.DeleteMetaInfo(id, isLoadedFromDisk);
    }
}