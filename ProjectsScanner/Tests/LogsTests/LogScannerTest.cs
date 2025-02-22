using ProjectsScanner.Scanners.ProjectsLogs;
using Xunit;

namespace ProjectsScanner.Tests.LogsTests;


public class LogScannerTest : IDisposable
{
    private string code = @"
    public class ClassWithLogs
    {
        ILogger<ClassWithLogs> _logger;

        public ClassWithLogs(ILogger<ClassWithLogs> logger)
        {
            _logger = logger;
        }
        
        public void Run()
        {
            string a = ""variable"";
            
            _logger.LogInformation(""Hello World!"");
            
            _logger.LogInformation($""Hello {a}"");
        }
    }
    ";

    private string expectedResult = @"
ClassWithLogs :
    Run :
      Hello World!,
      Hello {...}";
    
    
    public LogScannerTest()
    {
        
    }
    
    [Fact]
    public void FindLogsRow()
    {
        LogsAnalyzer logsAnalyzer = new LogsAnalyzer(code);
        var logsNodes = logsAnalyzer.GetLoggingNodes(); 
        var result = LogsAnalyzer.GetView(logsNodes);
        Assert.Equal(expectedResult, result);
    }
    
    [Fact]
    public void CheckPotentialCalls()
    {
        LogsAnalyzer logsAnalyzer = new LogsAnalyzer(code);
        var logsNodes = logsAnalyzer.GetPotentialCalls("Hello World!"); 
        var result = LogsAnalyzer.GetView(logsNodes.ToList());
        Assert.Equal(2, logsNodes.Count());
    }

    public void Dispose()
    {
    }
}