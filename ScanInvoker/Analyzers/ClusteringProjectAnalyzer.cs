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

        return classNode.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Count(IsLogInvocation);
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
    
    // MEL extension methods are specific enough to accept on their own.
    private static readonly HashSet<string> UnambiguousLogMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "LogTrace", "LogDebug", "LogInformation", "LogWarning", "LogError", "LogCritical"
    };

    // Serilog / NLog / log4net / nopCommerce ILogger level methods. Ambiguous names (many non-loggers
    // expose Error/Warning/...), so these count as a log only when the receiver looks like a logger.
    private static readonly HashSet<string> LevelLogMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "Log", "Trace", "Debug", "Info", "Information", "Warn", "Warning", "Error", "Critical", "Fatal", "Verbose",
        "TraceAsync", "DebugAsync", "InfoAsync", "InformationAsync", "WarnAsync", "WarningAsync",
        "ErrorAsync", "CriticalAsync", "FatalAsync", "VerboseAsync"
    };

    static bool IsLogInvocation(InvocationExpressionSyntax invocation)
    {
        var (methodName, receiver) = GetInvocationShape(invocation);
        if (string.IsNullOrEmpty(methodName)) return false;

        if (UnambiguousLogMethods.Contains(methodName)) return true;

        return LevelLogMethods.Contains(methodName) && IsLoggerReceiver(receiver);
    }
    
    private static ExpressionSyntax? GetFirstArgumentExpression(InvocationExpressionSyntax invocation)
    {
        return invocation?.ArgumentList?.Arguments.FirstOrDefault()?.Expression;
    }
    
    private static CallParametrizationStyle DetermineCallParametrizationStyle(InvocationExpressionSyntax invocation)
    {
        var templateArg = GetMessageTemplateArgument(invocation);
        if (templateArg == null) return CallParametrizationStyle.Other;

        if (templateArg is InterpolatedStringExpressionSyntax)
            return CallParametrizationStyle.Interpolation;

        if (templateArg is LiteralExpressionSyntax literal &&
            literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            return HasStructuredPlaceholder(literal.Token.ValueText)
                ? CallParametrizationStyle.Placeholder
                : CallParametrizationStyle.Other;
        }

        if (templateArg is BinaryExpressionSyntax binary &&
            binary.IsKind(SyntaxKind.AddExpression))
        {
            return CallParametrizationStyle.StringConcatenation;
        }

        if (IsJsonSerializationExpression(templateArg))
            return CallParametrizationStyle.JsonSerialization;

        // A bare variable / property / object / ToString() passed as the message is an unstructured
        // message, NOT JSON serialization (the old code mislabeled all of these as JsonSerialization).
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

        var templateArg = GetMessageTemplateArgument(invocation);
        var argsCount = invocation.ArgumentList?.Arguments.Count ?? 0;
        var style = DetermineCallParametrizationStyle(invocation);

        switch (style)
        {
            case CallParametrizationStyle.Placeholder:
                if (templateArg is LiteralExpressionSyntax lit &&
                    lit.IsKind(SyntaxKind.StringLiteralExpression))
                    return CountPlaceholdersInTemplate(lit.Token.ValueText);
                return Math.Max(0, argsCount - 1);

            case CallParametrizationStyle.Interpolation:
                if (templateArg is InterpolatedStringExpressionSyntax interpolated)
                    return interpolated.Contents.OfType<InterpolationSyntax>().Count();
                return Math.Max(0, argsCount - 1);

            case CallParametrizationStyle.StringConcatenation:
                if (templateArg is BinaryExpressionSyntax bin &&
                    bin.IsKind(SyntaxKind.AddExpression))
                    return CountConcatOperands(bin);
                return Math.Max(0, argsCount - 1);

            default:
                return Math.Max(0, argsCount - 1);
        }
    }

    private static (string methodName, string receiver) GetInvocationShape(InvocationExpressionSyntax invocation)
    {
        switch (invocation.Expression)
        {
            case MemberAccessExpressionSyntax memberAccess:
                return (memberAccess.Name.Identifier.Text, memberAccess.Expression.ToString());

            case MemberBindingExpressionSyntax memberBinding:
                var conditional = invocation.Parent?
                    .AncestorsAndSelf()
                    .OfType<ConditionalAccessExpressionSyntax>()
                    .FirstOrDefault();
                return (memberBinding.Name.Identifier.Text, conditional?.Expression.ToString() ?? string.Empty);

            default:
                return (null, string.Empty);
        }
    }

    private static bool IsLoggerReceiver(string receiver)
    {
        if (string.IsNullOrEmpty(receiver)) return false;

        var segment = receiver;

        int dot = segment.LastIndexOf('.');
        if (dot >= 0) segment = segment.Substring(dot + 1);

        int paren = segment.IndexOf('(');
        if (paren >= 0) segment = segment.Substring(0, paren);

        int generic = segment.IndexOf('<');
        if (generic >= 0) segment = segment.Substring(0, generic);

        segment = segment.Trim().TrimStart('_').ToLowerInvariant();

        return segment is "log" or "logger" or "logging"
               || segment.EndsWith("logger", StringComparison.Ordinal);
    }

    private static ExpressionSyntax GetMessageTemplateArgument(InvocationExpressionSyntax invocation)
    {
        var argList = invocation.ArgumentList;
        if (argList == null || argList.Arguments.Count == 0) return null;

        // MEL overloads place an Exception (and sometimes an EventId/LogLevel) before the message
        // template, so the template is the first string-literal or interpolated argument, not arg[0].
        foreach (var arg in argList.Arguments)
            if (arg.Expression is LiteralExpressionSyntax lit &&
                lit.IsKind(SyntaxKind.StringLiteralExpression))
                return arg.Expression;

        foreach (var arg in argList.Arguments)
            if (arg.Expression is InterpolatedStringExpressionSyntax)
                return arg.Expression;

        return argList.Arguments[0].Expression;
    }

    private static bool HasStructuredPlaceholder(string template)
    {
        if (string.IsNullOrEmpty(template)) return false;

        for (int i = 0; i < template.Length - 1; i++)
        {
            if (template[i] != '{') continue;
            if (template[i + 1] == '{') { i++; continue; }   // escaped {{

            int close = template.IndexOf('}', i + 1);
            if (close > i + 1) return true;                   // {something}
        }

        return false;
    }

    private static bool IsJsonSerializationExpression(ExpressionSyntax expression)
    {
        if (expression is not InvocationExpressionSyntax invocation)
            return false;

        var name = invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
            IdentifierNameSyntax identifier => identifier.Identifier.Text,
            _ => string.Empty
        };

        return name is "Serialize" or "SerializeObject" or "ToJson";
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