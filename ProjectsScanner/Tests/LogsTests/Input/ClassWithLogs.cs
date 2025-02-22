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
        var someClass = new SomeDataClass(0, "some string");
        string a = "variable";
        
        _logger.LogInformation("Hello World!");
        
        _logger.LogInformation($"Hello {a}");

        _logger.LogInformation($"Some class {someClass}");
    }

    private class SomeDataClass
    {
        private int someInt;
        private string someString;
        public SomeDataClass(int someInt, string someString)
        {
            this.someInt = someInt;
            this.someString = someString;
        }

        public string GetStringPlusInt()
        {
            return someString + " " + someInt;
        }
    }
}