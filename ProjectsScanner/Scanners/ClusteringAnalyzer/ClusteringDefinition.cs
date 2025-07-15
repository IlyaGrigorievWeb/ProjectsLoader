using System.Collections;
using Microsoft.CodeAnalysis;

namespace ProjectsScanner.Scanners.ClusteringAnalyzer;

class ClusteringDefinition<T> : IClusteringDefinition<T>
{
    private interface ITrigger
    {
        void Apply(SyntaxNode node);

        void Complete(T model);
    }

    class ClusteringTrigger<TV, TAgg>(
        Predicate<SyntaxNode> triggerPredicate,
        Func<SyntaxNode, TV> transformFunc,
        TAgg aggregationState,
        Func<TAgg, TV, TAgg> aggregatorFunc,
        Action<T, TAgg> resultMappingFunction
    ) : ITrigger
    {
        private TAgg _aggregationState = aggregationState;

        public void Apply(SyntaxNode node)
        {
            if (triggerPredicate(node))
            {
                var transformedNode = transformFunc(node);
                _aggregationState = aggregatorFunc(_aggregationState, transformedNode);
            }
        }

        public void Complete(T model)
        {
            resultMappingFunction(model, aggregationState);
        }
    }

    private List<ITrigger> _triggers = new();

    public void ApplyTriggers(SyntaxNode syntaxNode)
    {
        foreach (var trigger in _triggers)
        {
            trigger.Apply(syntaxNode);
        }
    }

    public void Complete(T model)
    {
        foreach (var trigger in _triggers)
        {
            trigger.Complete(model);
        }
    }

    private class TriggerBuilder(
        ClusteringDefinition<T> clusteringDefinition,
        Predicate<SyntaxNode> predicate
    ) : IClusteringDefinition<T>.ITriggerBuilder
    {
        /**
         *  Registers a transform function that maps SyntaxNode to some value, could be for example an int for maintaining
         *  a counter
         * <TV> - Mapped SyntaxNode (e.g. extracted data)
         */
        public IClusteringDefinition<T>.ITransformBuilder<TV> Transform<TV>(Func<SyntaxNode, TV> transformFunc)
        {
            return new TransformBuilder<TV>(clusteringDefinition, predicate, transformFunc);
        }
    }

    private class TransformBuilder<TV>(
        ClusteringDefinition<T> clusteringDefinition,
        Predicate<SyntaxNode> triggerPredicate,
        Func<SyntaxNode, TV> transformFunc
    ) : IClusteringDefinition<T>.ITransformBuilder<TV>
    {
        /**
         * Reduces all intermediate transformations into 1 result object <TR>, maintaining running aggregation of <TAgg>
         * and all intermediate transformations <TV>
         */
        public IClusteringDefinition<T>.ITerminalResultBuilder<TAgg> Fold<TAgg>(TAgg state,
            Func<TAgg, TV, TAgg> aggregator)
        {
            return new TerminalResultBuilder<TV, TAgg>(clusteringDefinition, triggerPredicate, transformFunc, state,
                aggregator);
        }
    }

    private class TerminalResultBuilder<TV, TAgg>(
        ClusteringDefinition<T> clusteringDefinition,
        Predicate<SyntaxNode> triggerPredicate,
        Func<SyntaxNode, TV> transformFunc,
        TAgg state,
        Func<TAgg, TV, TAgg> aggregator
    ) : IClusteringDefinition<T>.ITerminalResultBuilder<TAgg>
    {
        public IClusteringDefinition<T> MapResult(Action<T, TAgg> mappingFunc)
        {
            var trigger =
                new ClusteringTrigger<TV, TAgg>(triggerPredicate, transformFunc, state, aggregator, mappingFunc);

            clusteringDefinition._triggers.Add(trigger);

            return clusteringDefinition;
        }
    }

    /**
     * Registers a predicate condition that filters out irrelevant SyntaxNode's
     */
    public IClusteringDefinition<T>.ITriggerBuilder Trigger(Predicate<SyntaxNode> condition)
    {
        return new TriggerBuilder(this, condition);
    }
}