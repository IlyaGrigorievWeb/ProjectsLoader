using ProjectsLoader.Models;
using ScanInvoker.Models;

namespace ScanInvoker.Interfaces;

public interface IProjectAnalyzer
{
    ClassStats RunAnalyzer(string solutionRoot, CancellationToken cancellationToken = default);
    
    ProjectStatsClass RunTestAnalyzer(string solutionRoot, CancellationToken cancellationToken = default);
}