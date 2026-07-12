using ProjectsLoader.Models;
using ScanInvoker.Models;

namespace ScanInvoker.Interfaces;

public interface IProjectAnalyzer
{
    ProjectStatsClass RunAnalyzer(string solutionRoot, CancellationToken cancellationToken = default);
}