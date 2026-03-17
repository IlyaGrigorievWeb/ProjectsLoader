using Contracts.DataAnalisysEntities;
using ScanInvoker.Interfaces;
using ScanInvoker.Models;

namespace ScanInvoker.Services;

public class ProjectClusteringService : IProjectClusteringService
{
    public Task<ProjectClusteringInfo> Calculate(ProjectStatsClass projectStats, CancellationToken cancellationToken = default)
    {
        if (projectStats is null) throw new ArgumentNullException(nameof(projectStats));
            cancellationToken.ThrowIfCancellationRequested();

        var result = new ProjectClusteringInfo();

        int totalLogs = Math.Max(0, projectStats.TotalLogs);
        
        var rawCounts = new Dictionary<CallParametrizationStyle, int>
        {
            [CallParametrizationStyle.Placeholder] = Math.Max(0, projectStats.PlaceholderLogs),
            [CallParametrizationStyle.StringConcatenation] = Math.Max(0, projectStats.StringConcatenationLogs),
            [CallParametrizationStyle.Interpolation] = Math.Max(0, projectStats.InterpolationLogs),
            [CallParametrizationStyle.JsonSerialization] = Math.Max(0, projectStats.JsonSerializationLogs),
            [CallParametrizationStyle.Other] = Math.Max(0, projectStats.OtherLogs)
        };
        
        if (totalLogs == 0)
        {
            foreach (var kv in rawCounts)
                result.CallParametrizationStylesUsages[kv.Key] = 0.0;
        }
        else
        {
            var dict = new Dictionary<CallParametrizationStyle, double>();
            foreach (var kv in rawCounts)
                dict[kv.Key] = (double)kv.Value / totalLogs;
            
            var sum = dict.Values.Sum();
            if (sum > 0.0 && Math.Abs(sum - 1.0) > 1e-9)
            {
                var inv = 1.0 / sum;
                foreach (var key in dict.Keys.ToList())
                    dict[key] = dict[key] * inv;
            }

            result.CallParametrizationStylesUsages = dict;
        }
        
        result.MeanCallsPerMethod = projectStats.TotalClassCount > 0
            ? (double)projectStats.TotalLogs / projectStats.TotalClassCount
            : 0.0;
        
        result.TryCatchUsage = projectStats.TotalClassCount > 0
            ? projectStats.LogsInTryCatch / (double)projectStats.TotalLogs
            : 0.0;
        
        result.MeaningfulClassesUsage = projectStats.TotalClassCount > 0
            ? (double)projectStats.LogsClassCount / projectStats.TotalClassCount
            : 0.0;
        
        result.MidParametersCount = projectStats.TotalLogs > 0
            ? (double)projectStats.TotalLogs / projectStats.ParametresInLogs
            : 0;

        return Task.FromResult(result);
    }
}