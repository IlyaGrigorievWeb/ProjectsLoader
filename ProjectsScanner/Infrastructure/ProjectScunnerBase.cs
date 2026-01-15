using ProjecrScanner.CLI;

namespace ProjectsScanner.Infrastructure;

/// <summary>
/// API for Scunners which apply an Analyzer logic to type matched files
/// </summary>
public abstract class ProjectScunnerBase
{
    //TODO: Raw API, moved from the MVP console app
    public abstract void ProcessFile(string filePath, string solutionRoot, List<LogCallBaseEntry> result, HashSet<string> patterns);
    public abstract void ProcessFile(string filePath, string solutionRoot, List<LogCallBaseEntry> result, string outLogText);
    public abstract List<string> GetTargetFiles(string rootDir, HashSet<string> excludedDirs);
}