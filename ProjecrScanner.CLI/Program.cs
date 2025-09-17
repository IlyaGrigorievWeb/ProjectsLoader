
using ProjectsScanner.Infrastructure;
using ProjectsScanner.Scanners;
using ProjectsScanner.Scanners.ClusteringAnalyzer;
using ProjectsScanner.Scanners.ProjectsLogs;

//POC CLI App for "go to" feature testing
string solutionRoot = Directory.GetCurrentDirectory();
Console.WriteLine("Running parser in: " + solutionRoot);

var analyzer = new ProjectSlicer(
    new DotNetProjectScunner<LogsAnalyzer, List<LoggerCallNode>>(
        new LogsAnalyzer())); //TODO Doesn't work without setSearchText

// var analyzer = new ProjectSlicer(
//     new DotNetProjectScunner<ClusteringAnalyzer<List<LoggerCallNode>>, List<LoggerCallNode>>(
//         new ClusteringAnalyzer<List<LoggerCallNode>>(
//             )));

Console.WriteLine("\n--- Logging calls  ---");
foreach (var entry in analyzer.Analyze(solutionRoot))
{
    Console.WriteLine($"[{entry.LogCall}]\n => {entry.Context}\n => {entry.Location}\n");
}
Console.WriteLine("Enter log row");
string log = Console.ReadLine()!;
var results = analyzer.Search(solutionRoot, log);

Console.WriteLine("\n--- Matches ---");
foreach (var entry in results)
{
    Console.WriteLine($"[{entry.LogCall}]\n => {entry.Context}\n => {entry.Location}\n");
}
