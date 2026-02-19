using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ProjectsLoader.Models;
using ProjectsScanner.Scanners;
using ProjectsScanner.Scanners.ClusteringAnalyzer;
using ScanInvoker.Interfaces;
using ScanInvoker.Models;

namespace ScanInvoker.Analyzers;

public class ClusteringProjectAnalyzer : IProjectAnalyzer
{
    public ProjectStatsClass RunAnalyzer(string solutionRoot, CancellationToken cancellationToken = default)
    {
        var analyzer =
            new DotNetProjectScunner<ClusteringAnalyzer<ProjectStatsClass>, ProjectStatsClass>(
                new ClusteringAnalyzer<ProjectStatsClass>(new List<IClusteringDefinition<ProjectStatsClass>>()
                {
                    IClusteringDefinition<ProjectStatsClass>.Builder()
                        .Trigger(syntaxNode => syntaxNode is ClassDeclarationSyntax)
                        .Transform(_ => 1)
                        .Fold(0, (total, method) => total + method)
                        .MapResult((model, totalMethods) => model.TotalClassCount = totalMethods)
                    
                        .Trigger(syntaxNode => syntaxNode is ClassDeclarationSyntax)
                        .Transform(syntaxNode => CountLogInvocations((ClassDeclarationSyntax)syntaxNode))
                        .Fold(0, (total, logsInClass) => total + logsInClass)
                        .MapResult((model, totalLogsInClass) => model.LogsClassCount = totalLogsInClass)
                    
                        .Trigger(syntaxNode => syntaxNode is ClassDeclarationSyntax)
                        .Transform(syntaxNode => CountInterpolationLogs((ClassDeclarationSyntax)syntaxNode))
                        .Fold(0, (total, value) => total + value)
                        .MapResult((model, total) => model.InterpolationLogs = total)
                    
                        .Trigger(syntaxNode => syntaxNode is ClassDeclarationSyntax)
                        .Transform(syntaxNode => CountPlaceholderLogs((ClassDeclarationSyntax)syntaxNode))
                        .Fold(0, (total, value) => total + value)
                        .MapResult((model, total) => model.PlaceholderLogs = total)
                    
                        .Trigger(node => node is ClassDeclarationSyntax)
                        .Transform(node => CountConcatenationLogs((ClassDeclarationSyntax)node))
                        .Fold(0, (total, value) => total + value)
                        .MapResult((model, total) => model.StringConcatenationLogs = total)
                    
                        .Trigger(node => node is ClassDeclarationSyntax)
                        .Transform(node => CountJsonSerializationLogs((ClassDeclarationSyntax)node))
                        .Fold(0, (total, value) => total + value)
                        .MapResult((model, total) => model.JsonSerializationLogs = total)
                        
                        .Trigger(node => node is ClassDeclarationSyntax)
                        .Transform(node => CountOtherLogs((ClassDeclarationSyntax)node))
                        .Fold(0, (total, value) => total + value)
                        .MapResult((model, total) => model.OtherLogs = total)
                    
                        .Trigger(node => node is ClassDeclarationSyntax)
                        .Transform(node => CountAllLogs((ClassDeclarationSyntax)node))
                        .Fold(0, (total, value) => total + value)
                        .MapResult((model, total) => model.TotalLogs = total)

                }, ProjectStatsClass.NewInstance));
        
        return analyzer.RunAnalyzer(solutionRoot);
    }
    
    
    public static int CountLogInvocations(ClassDeclarationSyntax classNode)
    {
        if (classNode == null) return 0;

        var count = 0;
        
        var logMethodNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "LogInformation", "LogWarning", "LogError", "LogDebug", "LogTrace", "LogCritical",
            "Information", "Warning", "Error", "Debug", "Trace", "Fatal", "Verbose"
        };

        var invocations = classNode.DescendantNodes().OfType<InvocationExpressionSyntax>();

        foreach (var invocation in invocations)
        {
            var expr = invocation.Expression;
            string methodName = null!;
            string receiverText = string.Empty;

            if (expr is MemberAccessExpressionSyntax memberAccess)
            {
                methodName = memberAccess.Name.ToString();
                receiverText = memberAccess.Expression.ToString();
            }
            else if (expr is MemberBindingExpressionSyntax memberBinding)
            {
                methodName = memberBinding.Name.ToString();
                var cond = invocation.Parent?.AncestorsAndSelf().OfType<ConditionalAccessExpressionSyntax>().FirstOrDefault();
                receiverText = cond?.Expression.ToString() ?? string.Empty;
            }
            else if (expr is IdentifierNameSyntax identifier)
            {
                methodName = identifier.Identifier.Text;
                receiverText = identifier.ToString();
            }
            else
            {
                methodName = expr.ToString();
            }

            bool isLogCall = false;
            
            if (!string.IsNullOrEmpty(methodName))
            {
                if (logMethodNames.Contains(methodName) || methodName.IndexOf("Log", StringComparison.OrdinalIgnoreCase) >= 0)
                    isLogCall = true;
            }
            
            if (!isLogCall && !string.IsNullOrEmpty(receiverText))
            {
                if (receiverText.IndexOf("logger", StringComparison.OrdinalIgnoreCase) >= 0
                    || receiverText.IndexOf("log", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    isLogCall = true;
                }
            }

            if (isLogCall) count++;
        }

        return count;
    }
    
    static int CountLogInvocationsByStyle(
        ClassDeclarationSyntax classNode,
        CallParametrizationStyle style)
    {
        if (classNode == null) return 0;

        int count = 0;

        var invocations = classNode.DescendantNodes()
            .OfType<InvocationExpressionSyntax>();

        foreach (var invocation in invocations)
        {
            if (!IsLogInvocation(invocation))
                continue;

            var callStyle = DetermineCallParametrizationStyle(invocation);

            if (callStyle == style)
                count++;
        }

        return count;
    }
    
    static bool IsLogInvocation(InvocationExpressionSyntax invocation)
    {
        var expr = invocation.Expression;

        if (expr is MemberAccessExpressionSyntax memberAccess)
        {
            var name = memberAccess.Name.Identifier.Text;

            return name.Contains("Log", StringComparison.OrdinalIgnoreCase)
                   || name.Equals("Information", StringComparison.OrdinalIgnoreCase)
                   || name.Equals("Warning", StringComparison.OrdinalIgnoreCase)
                   || name.Equals("Error", StringComparison.OrdinalIgnoreCase)
                   || name.Equals("Debug", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }
    
    private static ExpressionSyntax? GetFirstArgumentExpression(InvocationExpressionSyntax invocation)
    {
        return invocation?.ArgumentList?.Arguments.FirstOrDefault()?.Expression;
    }
    
    private static CallParametrizationStyle DetermineCallParametrizationStyle(InvocationExpressionSyntax invocation)
        {
            var firstArg = GetFirstArgumentExpression(invocation);
            if (firstArg == null) return CallParametrizationStyle.Other;

            // interpolation $"..."
            if (firstArg is InterpolatedStringExpressionSyntax)
                return CallParametrizationStyle.Interpolation;

            // literal string with placeholders "Hello {name}"
            if (firstArg is LiteralExpressionSyntax literal &&
                literal.IsKind(SyntaxKind.StringLiteralExpression))
            {
                var text = literal.Token.ValueText;
                if (text.Contains("{") && text.Contains("}"))
                    return CallParametrizationStyle.Placeholder;

                return CallParametrizationStyle.Other;
            }

            // concatenation "a" + b
            if (firstArg is BinaryExpressionSyntax binary &&
                binary.IsKind(SyntaxKind.AddExpression))
            {
                return CallParametrizationStyle.StringConcatenation;
            }

            // inner invocation like obj.ToString()
            if (firstArg is InvocationExpressionSyntax innerInvocation)
            {
                if (innerInvocation.Expression is MemberAccessExpressionSyntax innerMember &&
                    string.Equals(innerMember.Name.Identifier.Text, "ToString", StringComparison.OrdinalIgnoreCase))
                {
                    return CallParametrizationStyle.JsonSerialization;
                }
            }

            // member / identifier / new Obj() / arr[index] -> object passed -> json-style
            if (firstArg is MemberAccessExpressionSyntax
                || firstArg is IdentifierNameSyntax
                || firstArg is ObjectCreationExpressionSyntax
                || firstArg is ElementAccessExpressionSyntax)
            {
                return CallParametrizationStyle.JsonSerialization;
            }

            return CallParametrizationStyle.Other;
        }
    
    static int CountInterpolationLogs(ClassDeclarationSyntax classNode)
        => CountLogInvocationsByStyle(classNode, CallParametrizationStyle.Interpolation);

    static int CountPlaceholderLogs(ClassDeclarationSyntax classNode)
        => CountLogInvocationsByStyle(classNode, CallParametrizationStyle.Placeholder);

    static int CountConcatenationLogs(ClassDeclarationSyntax classNode)
        => CountLogInvocationsByStyle(classNode, CallParametrizationStyle.StringConcatenation);

    static int CountJsonSerializationLogs(ClassDeclarationSyntax classNode)
        => CountLogInvocationsByStyle(classNode, CallParametrizationStyle.JsonSerialization);
    
    static int CountOtherLogs(ClassDeclarationSyntax classNode)
        => CountLogInvocationsByStyle(classNode, CallParametrizationStyle.Other);
    
    static int CountAllLogs(ClassDeclarationSyntax classNode)
        => CountLogInvocations(classNode);

}
