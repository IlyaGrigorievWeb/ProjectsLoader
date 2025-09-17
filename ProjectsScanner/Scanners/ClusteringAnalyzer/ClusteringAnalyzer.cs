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

        var nodeQueue = new Queue<SyntaxNode>();

        // enqueue all class declarations and start from there

        foreach (var classNode in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            nodeQueue.Enqueue(classNode);
        }

        while (nodeQueue.Any())
        {
            var node = nodeQueue.Dequeue();

            foreach (var definition in definitions)
            {
                definition.ApplyTriggers(node);
            }

            foreach (var child in node.DescendantNodes())
            {
                nodeQueue.Enqueue(child);
            }
        }

        var model = newInstanceFunc();
        
        foreach (var definition in definitions)
        {
            definition.Complete(model);
        }

        return model;
    }

    public T Analyze(string plainText)
    {
        throw new NotImplementedException();
    }
}