namespace ProjectsScannerTests.Features;

public class ClusteringModel
{
    
    public class MeanAggregation
    {
        public int Count { get; set; } = 0;
        public int Sum { get; set; } = 0;
    }
    
    public double AverageLogInvocationsPerMethod { get; set; } = 0.0;
    public int LogInvocationsCount { get; set; } = 0;
}