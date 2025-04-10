using Contracts.Entities;

namespace ProjectsScanner.Scanners;

public interface IWebPagesScanner
{
    public Task<GitHubProject> GetGitHubProject(string url);
}