using Xunit;

namespace ProjectsScanner.Tests.LogsTests;


public class LogScannerTest : IDisposable
{
    private string _logScannerClass = "main() { }";
    
    public LogScannerTest()
    {
        
    }
    
    [Fact]
    public void FindLogsRow()
    {
        Assert.NotEmpty(_logScannerClass);
    }

    public void Dispose()
    {
    }
}