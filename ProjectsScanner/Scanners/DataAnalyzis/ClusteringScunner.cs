using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ProjectsLoader.Models;
using ProjectsScanner.Scanners.ClusteringAnalyzer;
using ProjectsScanner.Scanners.ProjectsLogs;

namespace ProjectsScanner.Scanners.DataAnalyzis;

//TODO POC example, remove it
public class ClusteringScunner
{
    public void start()
    {
        //definition is one parsing rule for a one field of target model
        var definition = IClusteringDefinition<ClusteringModel>.Builder()
            .Trigger(syntaxNode => syntaxNode is ClassDeclarationSyntax) //Target Roslyn syntax node for trigger
            .Transform(syntaxNode => CountNumberOfLogInvocations((ClassDeclarationSyntax)syntaxNode)) //Parsing logic for founded syntax node 
            .Fold(new ClusteringModel.Aggregation(), ( totalStateForCalculationSession,logsCountInClass ) => //Fill aggregation model by data from parsing step
            {
                totalStateForCalculationSession.totalClassCount++;
                if (logsCountInClass > 0)
                {
                    totalStateForCalculationSession.LogsClassCount++;
                }

                return totalStateForCalculationSession;
            }).MapResult((model, aggregation) => //
            {
                model.MeanningfullClassesUsage = (double)aggregation.LogsClassCount/aggregation.totalClassCount;
            });

        var definition2 = IClusteringDefinition<ClusteringModel>.Builder()
            .Trigger(syntaxNode => syntaxNode is ClassDeclarationSyntax)
            .Transform(syntaxNode => CountNumberOfLogInvocations((ClassDeclarationSyntax)syntaxNode))
            .Fold(new ClusteringModel.Aggregation(), (totalStateForCalculationSession, logsCountInClass) =>
            {
                totalStateForCalculationSession.totalClassCount++;
                if (logsCountInClass > 0)
                {
                    totalStateForCalculationSession.LogsClassCount++;
                }

                return totalStateForCalculationSession;
            }).MapResult((model, aggregation) =>
            {
                model.MeanningfullClassesUsage = (double)aggregation.LogsClassCount / aggregation.totalClassCount;
            });
        
        var analyzer = new ClusteringAnalyzer<ClusteringModel>(new List<IClusteringDefinition<ClusteringModel>>() {definition, definition2}, () => new ClusteringModel());
        analyzer.Analyse("C# code in string");
    }
    
    
    //Not real logic, just example from the LogsProjectScunner
    static int CountNumberOfLogInvocations(ClassDeclarationSyntax methodDeclarationNode)
    {
        int result = 0;
        foreach (var classNode in methodDeclarationNode.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            var methodNodes = classNode.DescendantNodes().OfType<MethodDeclarationSyntax>();

            foreach (var methodNode in methodNodes)
            {
                // Extract all invocation expressions within the method body
                var invocations = methodNode.DescendantNodes().OfType<InvocationExpressionSyntax>();

                foreach (var invocation in invocations)
                {
                    // Check if the invocation matches the pattern [...].[...log...](...)
                    var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
                    if (
                        //memberAccess.Name.Identifier.Text.Contains("log", StringComparison.OrdinalIgnoreCase)
                        //invocation.Parent!.GetText().ToString().Contains("_logger", StringComparison.OrdinalIgnoreCase)
                        memberAccess != null &&
                        (invocation.Parent!.GetText().ToString()
                             .Contains("logger.Information", StringComparison.OrdinalIgnoreCase)
                         || invocation.Parent!.GetText().ToString()
                             .Contains("logger.Warning", StringComparison.OrdinalIgnoreCase)
                         || invocation.Parent!.GetText().ToString()
                             .Contains("logger.Error", StringComparison.OrdinalIgnoreCase)
                         || invocation.Parent!.GetText().ToString()
                             .Contains("logger.Debug", StringComparison.OrdinalIgnoreCase)
                        )
                    )
                    {
                        result++;
                    }
                }
            }
        }
        return result;
    }
}