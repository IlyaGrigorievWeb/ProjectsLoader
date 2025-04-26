using Microsoft.CodeAnalysis;

namespace ProjectsScanner.Scanners.ClusteringAnalyzer;

public interface IClusteringDefinition<T>
{
    public static IClusteringDefinition<T> Builder() => new ClusteringDefinition<T>();
    
    ITriggerBuilder Trigger(Predicate<SyntaxNode> condition);

    void ApplyTriggers(SyntaxNode syntaxNode);

    void Complete(T model);

    public interface IBiConsumer<T1, T2>
    {
        public void Consume(T1 first, T2 second);
    }
    
    public interface ITriggerBuilder
    {
        ITransformBuilder<TV> Transform<TV>(Func<SyntaxNode, TV> func);
    }
    
    public interface ITransformBuilder<TV>
    {
        ITerminalResultBuilder<TAgg> Fold<TAgg>(TAgg state, Func<TAgg, TV, TAgg> aggregator);
    }
    
    public interface ITerminalResultBuilder<TAgg>
    {
        IClusteringDefinition<T> MapResult(Action<T, TAgg> mappingFunc);
    }
}