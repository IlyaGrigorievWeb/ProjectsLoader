using Contracts.DataAnalisysEntities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
                        
                        .Trigger(node => node is CatchClauseSyntax)
                        .Transform(node => CountLogsInTryCatch((CatchClauseSyntax)node))
                        .Fold(0, (total, value) => total + value)
                        .MapResult((model, total) => model.LogsInTryCatch = total)

                        .Trigger(node => node is InvocationExpressionSyntax)
                        .Transform(node => CountParametersInLog((InvocationExpressionSyntax)node))
                        .Fold(0, (total, value) => total + value)
                        .MapResult((model, total) => model.ParametresInLogs = total)
                    
                        .Trigger(node => node is ClassDeclarationSyntax)
                        .Transform(node => HasLogInClass((ClassDeclarationSyntax)node))
                        .Fold(0, (total, value) => total + value)
                        .MapResult((model, total) => model.ClassesWithLogs = total)

                        .Trigger(node => node is MethodDeclarationSyntax)
                        .Transform(node => HasLogInMethod((MethodDeclarationSyntax)node))
                        .Fold(0, (total, value) => total + value)
                        .MapResult((model, total) => model.MethodsWithLogs = total)

                        .Trigger(node => node is MethodDeclarationSyntax)
                        .Transform(node => CountLogsInMethodIfHasLogs((MethodDeclarationSyntax)node))
                        .Fold(0, (total, value) => total + value)
                        .MapResult((model, total) => model.TotalLogsInMethodsWithLogs = total)

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
        
        if (firstArg is InterpolatedStringExpressionSyntax)
            return CallParametrizationStyle.Interpolation;
        
        if (firstArg is LiteralExpressionSyntax literal &&
            literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            var text = literal.Token.ValueText;
            if (text.Contains("{") && text.Contains("}"))
                return CallParametrizationStyle.Placeholder;

            return CallParametrizationStyle.Other;
        }
        
        if (firstArg is BinaryExpressionSyntax binary &&
            binary.IsKind(SyntaxKind.AddExpression))
        {
            return CallParametrizationStyle.StringConcatenation;
        }
        
        if (firstArg is InvocationExpressionSyntax innerInvocation)
        {
            if (innerInvocation.Expression is MemberAccessExpressionSyntax innerMember &&
                string.Equals(innerMember.Name.Identifier.Text, "ToString", StringComparison.OrdinalIgnoreCase))
            {
                return CallParametrizationStyle.JsonSerialization;
            }
        }
        
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
    
    static int CountLogsInTryCatch(CatchClauseSyntax catchNode)
    {
        if (catchNode == null) return 0;
        
        var block = catchNode.Block;
        if (block == null) return 0;

        var invocations = block.DescendantNodes().OfType<InvocationExpressionSyntax>();
        int count = 0;

        foreach (var invocation in invocations)
        {
            if (IsLogInvocation(invocation))
                count++;
        }

        return count;
    }
    
    static int CountParametersInLog(InvocationExpressionSyntax invocation)
    {
        if (!IsLogInvocation(invocation))
            return 0;

        var argsCount = invocation.ArgumentList?.Arguments.Count ?? 0;
        var firstArg = GetFirstArgumentExpression(invocation);
        var style = DetermineCallParametrizationStyle(invocation);

        switch (style)
        {
            case CallParametrizationStyle.Placeholder:
            {
                if (firstArg is LiteralExpressionSyntax lit &&
                    lit.IsKind(SyntaxKind.StringLiteralExpression))
                {
                    int placeholders = CountPlaceholdersInTemplate(lit.Token.ValueText);
                    return Math.Max(placeholders, Math.Max(0, argsCount - 1));
                }

                return Math.Max(0, argsCount - 1);
            }

            case CallParametrizationStyle.Interpolation:
            {
                if (firstArg is InterpolatedStringExpressionSyntax interpolated)
                {
                    int interpCount = interpolated.Contents
                        .OfType<InterpolationSyntax>()
                        .Count();

                    return Math.Max(interpCount, Math.Max(0, argsCount - 1));
                }

                return Math.Max(0, argsCount - 1);
            }

            case CallParametrizationStyle.StringConcatenation:
            {
                if (firstArg is BinaryExpressionSyntax bin &&
                    bin.IsKind(SyntaxKind.AddExpression))
                {
                    return CountConcatOperands(bin);
                }

                return Math.Max(0, argsCount - 1);
            }

            case CallParametrizationStyle.JsonSerialization:
            {
                return argsCount > 0 ? Math.Max(1, argsCount - 1) : 0;
            }

            default:
                return Math.Max(0, argsCount - 1);
        }
    }
    
    static int CountPlaceholdersInTemplate(string template)
    {
        if (string.IsNullOrEmpty(template))
            return 0;

        int count = 0;

        for (int i = 0; i < template.Length; i++)
        {
            if (template[i] == '{')
            {
                if (i + 1 < template.Length && template[i + 1] == '{')
                {
                    i++;
                    continue;
                }

                int closing = template.IndexOf('}', i + 1);
                if (closing > i)
                {
                    count++;
                    i = closing;
                }
            }
        }

        return count;
    }
    
    static int CountConcatOperands(BinaryExpressionSyntax binary)
    {
        int count = 0;

        void Traverse(ExpressionSyntax expr)
        {
            if (expr is BinaryExpressionSyntax bin &&
                bin.IsKind(SyntaxKind.AddExpression))
            {
                Traverse(bin.Left);
                Traverse(bin.Right);
            }
            else
            {
                if (!(expr is LiteralExpressionSyntax lit &&
                      lit.IsKind(SyntaxKind.StringLiteralExpression)))
                {
                    count++;
                }
            }
        }

        Traverse(binary);

        return count;
    }
    
    public static int HasLogInClass(ClassDeclarationSyntax classNode)
    {
        if (classNode == null) return 0;

        var invocations = classNode.DescendantNodes().OfType<InvocationExpressionSyntax>();

        foreach (var invocation in invocations)
        {
            if (IsLogInvocation(invocation))
                return 1;
        }

        return 0;
    }

    public static int HasLogInMethod(MethodDeclarationSyntax methodNode)
    {
        if (methodNode == null) return 0;

        var invocations = methodNode.DescendantNodes().OfType<InvocationExpressionSyntax>();

        foreach (var invocation in invocations)
        {
            if (IsLogInvocation(invocation))
                return 1;
        }

        return 0;
    }

    public static int CountLogsInMethodIfHasLogs(MethodDeclarationSyntax methodNode)
    {
        if (methodNode == null) return 0;

        var invocations = methodNode.DescendantNodes().OfType<InvocationExpressionSyntax>();
        int count = 0;

        foreach (var invocation in invocations)
        {
            if (IsLogInvocation(invocation))
                count++;
        }

        return count;
    }
}