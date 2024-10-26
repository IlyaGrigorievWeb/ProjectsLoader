using Contracts.Interfaces;

namespace Contracts.Entities;

public class GitHubProject
{
    public Guid Id { get; set; }

    /// <summary>
    /// https://github.com/[UrlPostfix]
    /// </summary>
    public string UrlPostfix { get; set; }

    public WebFrameworks WebFramework { get; set; }

    public int Stars { get; set; }

    public DateTime CreationDate { get; set; }

    public string DefaultBranch { get; set; }
}