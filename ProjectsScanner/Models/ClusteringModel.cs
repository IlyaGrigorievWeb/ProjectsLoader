namespace ProjectsLoader.Models;

//TODO POC example for first usage, remove it
public class ClusteringModel
{
    public class Aggregation()
    {
        public int totalClassCount = 0;
        public int LogsClassCount = 0;
    }

    public Aggregation agregation { get; set; } = new Aggregation();
    
    public double MeanningfullClassesUsage { get; set; }
}