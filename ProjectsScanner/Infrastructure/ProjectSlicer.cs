using ProjecrScanner.CLI;

namespace ProjectsScanner.Infrastructure;

/// <summary>
/// Wrapper for a Scunner, simple endpoint for apps with multiple scunners
/// </summary>
public class ProjectSlicer
{
    private readonly ProjectScunnerBase _projectScunner;

    public ProjectSlicer(ProjectScunnerBase projectScunner)
    {
        _projectScunner = projectScunner;
    }
    
    public List<LogCallBaseEntry> Analyze(string rootDir)
    {
        var result = new List<LogCallBaseEntry>();
        var patternsMap = new HashSet<string>();
        var excludedDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "bin", "obj", ".vs", ".git", "TestResults", "packages"
        };

        var csFiles = _projectScunner.GetTargetFiles(rootDir, excludedDirs);

        foreach (var file in csFiles)
        {
            _projectScunner.ProcessFile(file, rootDir, result, patternsMap);
        }

        return result;
    }
    public List<LogCallBaseEntry> Search(string rootDir, string outLogText)
    {
        var result = new List<LogCallBaseEntry>();
        var excludedDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "bin", "obj", ".vs", ".git", "TestResults", "packages"
        };

        var csFiles = _projectScunner.GetTargetFiles(rootDir, excludedDirs);

        foreach (var file in csFiles)
        {
            _projectScunner.ProcessFile(file, rootDir, result, outLogText);
        }

        return result;
    }
}