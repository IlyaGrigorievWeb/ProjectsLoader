using Microsoft.CodeAnalysis.CSharp.Syntax;
using ProjectsLoader.Models;
using ProjectsScanner.Infrastructure;
using ProjectsScanner.Scanners;
using ProjectsScanner.Scanners.ClusteringAnalyzer;
using ProjectsScanner.Scanners.ProjectsLogs;

//POC CLI App for "go to" feature testing
string solutionRoot = Directory.GetCurrentDirectory();
Console.WriteLine("Running parser in: " + solutionRoot);

// var analyzer = new ProjectSlicer(
//     new DotNetProjectScunner<LogsAnalyzer, List<LoggerCallNode>>(
//         new LogsAnalyzer())); //TODO Doesn't work without setSearchText

var analyzer =
    new DotNetProjectScunner<ClusteringAnalyzer<ClassStats>, ClassStats>(
        new ClusteringAnalyzer<ClassStats>(new List<IClusteringDefinition<ClassStats>>()
        {
            IClusteringDefinition<ClassStats>.Builder()
                .Trigger(syntaxNode => syntaxNode is MethodDeclarationSyntax)
                .Transform(_ => 1 /* each method contributes to 1 in total count */)
                .Fold(0, (total, method) => total + method)
                .MapResult((model, totalMethods) =>
                {
                    model.FunctionCount = totalMethods;
                })
                
                .Trigger(syntaxNode => syntaxNode is PropertyDeclarationSyntax)
                .Transform(_ => 1)
                .Fold(0, (property, total) => total + property)
                .MapResult((model, totalProperty) =>
                {
                    model.PropertyCount = totalProperty;
                })
        }, ClassStats.NewInstance));

Console.WriteLine("\n--- Logging calls  ---");
// foreach (var entry in analyzer.RunAnalyzer(solutionRoot))
{
    // Console.WriteLine($"[{entry.LogCall}]\n => {entry.Context}\n => {entry.Location}\n");
}

var analysisResult = analyzer.RunAnalyzer(solutionRoot);

if (analysisResult != null)
{
    Console.WriteLine($"Function count: {analysisResult.FunctionCount} PropertyCount: {analysisResult.PropertyCount}");
}

// Console.WriteLine("Enter log row");
// string log = Console.ReadLine()!;
// var results = analyzer.Search(solutionRoot, log);
//
// Console.WriteLine("\n--- Matches ---");
// foreach (var entry in results)
// {
//     Console.WriteLine($"[{entry.LogCall}]\n => {entry.Context}\n => {entry.Location}\n");
// }