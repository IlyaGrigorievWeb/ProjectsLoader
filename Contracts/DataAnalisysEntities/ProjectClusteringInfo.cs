using Contracts.Interfaces;

namespace Contracts.DataAnalisysEntities;

public class ProjectClusteringInfo
{
    public Dictionary<CallParametrizationStyle, double> CallParametrizationStylesUsages { get; set; } = new();
    public double MeanCallsPerMethod { get; set; }
    public double TryCatchUsage { get; set; }
    public double MeaningfulClassesUsage { get; set; }
    public double MidParametersCount  { get; set; }
}