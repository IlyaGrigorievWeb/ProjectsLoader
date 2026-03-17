using Contracts.Interfaces;

namespace Contracts.DataAnalisysEntities;

public class ProjectClusteringInfo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ProjectName { get; set; }
    public Dictionary<CallParametrizationStyle, double> CallParametrizationStylesUsages { get; set; } = new();
    public double MeanCallsPerMethod { get; set; }
    public double TryCatchUsage { get; set; }
    public double MeaningfulClassesUsage { get; set; }
    public double MidParametersCount  { get; set; }
}