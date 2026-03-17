using ProjectsScanner.Scanners;

namespace ScanInvoker.Models;

public class ProjectStatsClass : IMergeableModel<ProjectStatsClass>
{
    public int TotalClassCount = 0;
    public int LogsClassCount = 0;
    public int TotalLogs = 0;
    public int PlaceholderLogs = 0;
    public int StringConcatenationLogs = 0;
    public int InterpolationLogs = 0;
    public int JsonSerializationLogs = 0;
    public int LogsInTryCatch = 0;
    public int ParametresInLogs = 0;
    public int OtherLogs = 0;
    
    
    public static ProjectStatsClass NewInstance() => new ProjectStatsClass();
    
    public void Merge(ProjectStatsClass model)
    {
        TotalClassCount += model.TotalClassCount;
        LogsClassCount += model.LogsClassCount;
        TotalLogs += model.TotalLogs;
        PlaceholderLogs += model.PlaceholderLogs;
        StringConcatenationLogs += model.StringConcatenationLogs;
        InterpolationLogs += model.InterpolationLogs;
        JsonSerializationLogs += model.JsonSerializationLogs;
        OtherLogs += model.OtherLogs;
        LogsInTryCatch += model.LogsInTryCatch;
        ParametresInLogs += model.ParametresInLogs;
    }
    
}