using ProjectsScanner.Scanners.ProjectsLogs;
using ProjectsScannetTests.Framework;
using Xunit;
using Xunit.Abstractions;

namespace ProjectsScanner.Tests.LogsTests.Features;

public class BigCodeSmallLogTest : FileTestBase, IClassFixture<IOFilesFixture>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly string _code;
    private readonly string _logRow = "[02/11/2025 00:13:58] [Thread 1] Executing ConsolePrinter (Input: String, Output: Void)";

    public BigCodeSmallLogTest(IOFilesFixture testFilesStorage, ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _code = testFilesStorage.GetFileContent(InputFileName);
    }

    [Fact]
    public void GetView()
    {
        LogsAnalyzer logsAnalyzer = new LogsAnalyzer(_code);
        var logsNodes = logsAnalyzer.GetLoggingNodes();
        var result = LogsAnalyzer.FindBestMatchPosition(_code, _logRow);
        //[Thread {ThreadId}] Executing {Chain} (Input: {InputType}, Output:

        _testOutputHelper.WriteLine(result);

    }
    
}