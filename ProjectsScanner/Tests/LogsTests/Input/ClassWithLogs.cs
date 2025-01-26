using Microsoft.Extensions.Logging;

namespace ProjectsScanner.Tests.LogsTests.Input;

public class ClassWithLogs
{
    ILogger<ClassWithLogs> _logger;

    public ClassWithLogs(ILogger<ClassWithLogs> logger)
    {
        _logger = logger;
    }
    
    public void Run()
    {
        string a = "variable";
        
        _logger.LogInformation("Hello World!");
        
        _logger.LogInformation($"Hello {a}");
    }
}