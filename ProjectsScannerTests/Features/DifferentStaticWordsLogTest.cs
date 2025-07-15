using ProjectsScanner.Scanners.ProjectsLogs;
using ProjectsScannerTests.Framework;
using Xunit.Abstractions;

namespace ProjectsScannerTests.Features;

public class DifferentStaticWordsLogTest : FileTestBase, IClassFixture<IOFilesFixture>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly string _code;
    private readonly string _logRow = "";

    public DifferentStaticWordsLogTest(IOFilesFixture testFilesStorage, ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _code = testFilesStorage.GetFileContent(InputFileName);
    }

    //TODO Support asserts by gold file (_testOutputHelper)

    [Fact]
    public void GetView()
    {
        LogsAnalyzer logsAnalyzer = new LogsAnalyzer(_code);
        var logsNodes = logsAnalyzer.GetLoggingNodes();
        var result = LogsAnalyzer.GetPatternsHashMap(logsNodes);
        Assert.Equal(6, result.Count());
    }
}