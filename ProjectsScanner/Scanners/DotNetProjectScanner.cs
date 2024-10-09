using Contracts.Interfaces;

namespace ProjectsScanner.Scanners;

public class DotNetProjectScanner : ICodeScanner
{
    
    public WebFrameworks getProjectWebFramework()
    {
        throw new NotImplementedException();
    }

    public string getProjectLanguage()
    {
        throw new NotImplementedException();
    }

    public bool isLoggingSupported()
    {
        throw new NotImplementedException();
    }

    public string getLoggerFramework()
    {
        throw new NotImplementedException();
    }
}