using ProjecrScanner.CLI;
using ProjectsScanner.Infrastructure;
using ProjectsScanner.Scanners.ProjectsLogs;

namespace ProjectsScanner.Scanners;

public class DotNetProjectScunner<TAnalyzer, TOut>(
    TAnalyzer analyzer //TODO here an Analyzer must be as part of scunner configuration. Currently we use it per method call, need to change it
    ) : ProjectScunnerBase
    where TAnalyzer : IAnalyzer<TOut>
    where TOut : IMergeableModel<TOut>
{
    //private T _analyzer = analyzer;

    [Obsolete]
    public override void ProcessFile(string filePath, string solutionRoot, List<LogCallBaseEntry> result, HashSet<string> patterns)
    {
        string relativePath = Path.GetRelativePath(solutionRoot, filePath).Replace("/", "\\");
        string fileText = File.ReadAllText(filePath);
        string[] allLines = File.ReadAllLines(filePath);

        foreach (var classBlock in LogsAnalyzer.ExtractClassBlocks(fileText))
        {
            LogsAnalyzer logsAnalyzer = new LogsAnalyzer();
            
            var loggerCalls = logsAnalyzer.GetLoggingNodes(classBlock.Content);

            foreach (var pattern in LogsAnalyzer.GetPatternsHashMap(loggerCalls).Keys) 
                patterns.Add(pattern);

            foreach (var call in loggerCalls)
            {
                // Calculating of location
                string logCall = call.LogText.Trim();
                int lineIndex = LogsAnalyzer.FindLineIndexOfLog(allLines, logCall);

                var fullContext = $"{call.ClassName}.{call.MethodName}";
                string location = $"{relativePath}:{lineIndex + 1}";

                result.Add(new LogCallBaseEntry
                {
                    LogCall = logCall,
                    Context = fullContext,
                    Location = location
                });
            }
        }
    }
    
    [Obsolete]
    public override void ProcessFile(string filePath, string solutionRoot, List<LogCallBaseEntry> result, string outLogText)
    {
        string relativePath = Path.GetRelativePath(solutionRoot, filePath).Replace("/", "\\");
        string fileText = File.ReadAllText(filePath);
        string[] allLines = File.ReadAllLines(filePath);

        foreach (var classBlock in LogsAnalyzer.ExtractClassBlocks(fileText))
        {
            LogsAnalyzer logsAnalyzer = new LogsAnalyzer();

            var potentialCalls = new List<LoggerCallNode>();
            foreach (var pair in logsAnalyzer.GetPotentialCallsByPatterns(classBlock.Content, outLogText))
            {
                potentialCalls.AddRange(pair.Value);
            }

            foreach (var call in potentialCalls)
            {
                // Calculating of location
                string logCall = call.LogText.Trim();
                int lineIndex = LogsAnalyzer.FindLineIndexOfLog(allLines, logCall);

                var fullContext = $"{call.ClassName}.{call.MethodName}";
                string location = $"{relativePath}:{lineIndex + 1}";

                result.Add(new LogCallBaseEntry
                {
                    LogCall = logCall,
                    Context = fullContext,
                    Location = location
                });
            }
        }
    }
    
    [Obsolete]
    public override List<string> GetTargetFiles (string rootDir, HashSet<string> excludedDirs)
    {
        var files = new List<string>();
        var dirsToProcess = new Stack<string>();
        dirsToProcess.Push(rootDir);

        while (dirsToProcess.Count > 0)
        {
            var currentDir = dirsToProcess.Pop();
            try
            {
                foreach (var subDir in Directory.GetDirectories(currentDir))
                {
                    var dirName = Path.GetFileName(subDir);
                    if (!excludedDirs.Contains(dirName))
                    {
                        dirsToProcess.Push(subDir);
                    }
                }

                foreach (var file in Directory.GetFiles(currentDir, "*.cs"))
                {
                    files.Add(file);
                }
            }
            catch
            {
                // Skip strange dirs
            }
        }

        return files;
    }

    //TODO First try to use an analyzer from scunners. Currently just example of long-term solution
    public TOut? RunAnalyzer(string root)
    {
        var excludedDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "bin", "obj", ".vs", ".git", "TestResults", "packages"
        };

        var csFiles = GetTargetFiles(root, excludedDirs);

        TOut? resultModel = default(TOut);
        
        foreach (var filePath in csFiles)
        {
            string fileText = File.ReadAllText(filePath);
            
            foreach (var classBlock in LogsAnalyzer.ExtractClassBlocks(fileText))
            {
                var classResult = analyzer.Analyse(classBlock.Content);

                if (resultModel == null)
                {
                    resultModel = classResult;
                }
                else
                {
                    resultModel.Merge(classResult);
                }
            }
        }

        return resultModel;
    }
}