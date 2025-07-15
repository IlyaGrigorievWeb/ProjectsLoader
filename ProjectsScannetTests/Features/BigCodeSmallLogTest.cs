using Microsoft.CodeAnalysis.CSharp.Syntax;
using ProjectsScanner.Scanners.ClusteringAnalyzer;
using ProjectsScanner.Scanners.ProjectsLogs;
using ProjectsScannetTests.Features;
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
    
    //TODO Support asserts by gold file (_testOutputHelper)
    [Fact]
    public void GetView()
    {
        LogsAnalyzer logsAnalyzer = new LogsAnalyzer(_code);
        var logsNodes = logsAnalyzer.GetLoggingNodes();
        var patternsMap = LogsAnalyzer.GetPatternsHashMap(logsNodes);
        var result = logsAnalyzer.GetPotentialCalls(_logRow); 
        Assert.Equal(4, result.Count());
    }

    [Fact]
    public void TestClusteringAnalyzer()
    {
        var definition = IClusteringDefinition<ClusteringModel>.Builder()
            .Trigger(syntaxNode => syntaxNode is MethodDeclarationSyntax)
            .Transform(syntaxNode => CountNumberOfLogInvocations((MethodDeclarationSyntax)syntaxNode))
            .Fold(new ClusteringModel.MeanAggregation(), (aggregation, countPerMethod) =>
            {
                if (countPerMethod <= 0) return aggregation;
                aggregation.Count++;
                aggregation.Sum += countPerMethod;
                return aggregation;
            }).MapResult((model, aggregation) =>
            {
                if (aggregation.Count <= 0) return;
                model.AverageLogInvocationsPerMethod = aggregation.Sum / (double)aggregation.Count;
                model.LogInvocationsCount += aggregation.Sum;
            });

        var analyzer = new ClusteringAnalyzer<ClusteringModel>(definition, () => new ClusteringModel());

        var codeModel = analyzer.Analyse(_code);
        
        Assert.Equal(4, codeModel.LogInvocationsCount);
        _testOutputHelper.WriteLine($"Average log invocation is {codeModel.AverageLogInvocationsPerMethod}");
        _testOutputHelper.WriteLine($"Total invocation count is {codeModel.LogInvocationsCount}");
    }
    
    static int CountNumberOfLogInvocations(MethodDeclarationSyntax methodDeclarationNode)
    {
        int count = 0;

        foreach (var invocation in methodDeclarationNode.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            if (memberAccess != null &&
                memberAccess.Name.Identifier.Text.Contains("log", StringComparison.OrdinalIgnoreCase))
            {
                count++;
            }
        }

        return count;
    }
}