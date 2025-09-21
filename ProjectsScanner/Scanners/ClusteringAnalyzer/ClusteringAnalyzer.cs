using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ProjectsScanner.Infrastructure;

namespace ProjectsScanner.Scanners.ClusteringAnalyzer;

public class ClusteringAnalyzer<T>(
    IEnumerable<IClusteringDefinition<T>> definitions,
    Func<T> newInstanceFunc)
    : IAnalyzer<T>
{
    public T Analyse(string plainText)
    {
        var tree = CSharpSyntaxTree.ParseText(plainText);

        foreach (var node in tree.GetRoot().DescendantNodesAndSelf())
        {
            foreach (var definition in definitions)
            {
                definition.ApplyTriggers(node);
            }
        }

        var model = newInstanceFunc();
        
        foreach (var definition in definitions)
        {
            definition.Complete(model);
        }

        return model;
    }
}