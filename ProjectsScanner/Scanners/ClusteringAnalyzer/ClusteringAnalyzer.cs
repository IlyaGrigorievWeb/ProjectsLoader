using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ProjectsScanner.Scanners.ClusteringAnalyzer;

public class ClusteringAnalyzer<T>(
    IClusteringDefinition<T> definition,
    Func<T> newInstanceFunc
    )
{
    public T Analyse(string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code);

        var nodeQueue = new Queue<SyntaxNode>();

        // enqueue all class declarations and start from there

        foreach (var classNode in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            nodeQueue.Enqueue(classNode);
        }

        while (nodeQueue.Any())
        {
            var node = nodeQueue.Dequeue();
            
            definition.ApplyTriggers(node);

            foreach (var child in node.DescendantNodes())
            {
                nodeQueue.Enqueue(child);
            }
        }

        var model = newInstanceFunc();
        
        definition.Complete(model);

        return model;
    }
}