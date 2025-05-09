using Contracts.Entities;
using Contracts.Interfaces;

namespace ProjectsScanner.Scanners;

public class DotNetProjectScanner<T> : ICodeScanner<GitHubProject>
{
    
    public WebFrameworks getProjectWebFramework()
    {
        throw new NotImplementedException();
    }

    public string getProjectLanguage()
    {
        throw new NotImplementedException();
    }

    public GitHubProject isLoggingSupported()
    {
        throw new NotImplementedException();
    }

    public GitHubProject getLoggerFramework()
    {
        throw new NotImplementedException();
    }
}