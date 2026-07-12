using Contracts.DataAnalisysEntities;
using ScanInvoker.Models;

namespace ScanInvoker.Interfaces;

public interface IProjectClusteringService
{ 
    Task<ProjectClusteringInfo> Calculate(ProjectStatsClass  projectStats, CancellationToken cancellationToken = default);
}