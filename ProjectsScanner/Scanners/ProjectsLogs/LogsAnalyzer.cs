using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.ML;
using Microsoft.ML.Data;
using ProjectsScanner.Infrastructure;

namespace ProjectsScanner.Scanners.ProjectsLogs;

//TODO It's first version of a logs parser. Need to clean it and refactor to new architecture. 
//Need to remove this monolith or rebuild it to small container of logs related logic (for reusing)
[Obsolete]
public class LogsAnalyzer : IAnalyzer<List<LoggerCallNode>>
{
    //Tried to use ML MS libs and levenshtein distance. Regex is an optimal way for MVP of "LogsAnalyzer"
    private SyntaxNode _endpointNode = null; //TODO Better to use Roslyn tree here
    private string _searchText = ""; 

    //TODO rudiment for compatibility with IAnalyzer API, remove it
    public void SetAnalysisText(string text)
    {
        _searchText = text;
    }

    public List<LoggerCallNode> GetLoggingNodes(string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var result = new List<LoggerCallNode>();

        foreach (var classNode in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {

            var methodNodes = classNode.DescendantNodes().OfType<MethodDeclarationSyntax>();

            foreach (var methodNode in methodNodes)
            {
                var methodName = methodNode.Identifier.Text;

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
                        //TODO Don't process lambdas and calls in submethods, but triggered corectlu by condition
                        //TODO localization/ any wrapper for string message 
                        // Extract the argument passed to the logging call
                        var argumentList = invocation.ArgumentList.Arguments;
                        if (argumentList.Count > 0)
                        {
                            var argument = argumentList[0].Expression;
                            // Attempt to evaluate the argument to get the string value
                            string logMessage = null;

                            if (argument is LiteralExpressionSyntax literal &&
                                literal.IsKind(SyntaxKind.StringLiteralExpression))
                            {
                                logMessage = literal.Token.ValueText;
                            }
                            else if (argument is InterpolatedStringExpressionSyntax interpolatedString)
                            {
                                // Handle interpolated strings ($"...")
                                logMessage = string.Join("", interpolatedString.Contents.Select(content =>
                                {
                                    if (content is InterpolatedStringTextSyntax text)
                                    {
                                        return text.TextToken.ValueText;
                                    }

                                    return "{...}"; // Placeholder for expressions within interpolated strings
                                }));
                            }
                            else if (argument is LiteralExpressionSyntax verbatimLiteral &&
                                     verbatimLiteral.IsKind(SyntaxKind.StringLiteralExpression))
                            {
                                // Handle verbatim strings (@"...")
                                logMessage = verbatimLiteral.Token.ValueText;
                            }

                            //Saving over arguments
                            var overArguments = new Dictionary<string, string>();
                            var argumentsInOrder = new string[argumentList.Count - 1];
                            //Save arguments except 0 index (only arguments)
                            if (argumentList.Count > 1)
                            {
                                for (int i = 1; i < argumentList.Count; i++)
                                {
                                    //TODO: not implemented reading type of arguments argumentList[i].Expression.GetMemberType()
                                    overArguments[argumentList[i].GetText().ToString()] = "UnknownType";
                                    argumentsInOrder[i - 1] = argumentList[i].GetText().ToString();
                                }
                            }

                            if (!string.IsNullOrEmpty(logMessage))
                            {
                                result.Add(
                                    new LoggerCallNode
                                    {
                                        ClassName = classNode.Identifier.Text,
                                        MethodName = methodName,
                                        LogText = logMessage,
                                        ParametersInOrder = argumentsInOrder,
                                        Parameters = overArguments,
                                    });
                            }
                        }
                    }
                }
            }

        }

        return result;
    }

    public static Dictionary<string, List<LoggerCallNode>> GetPatternsHashMap(List<LoggerCallNode> callNodes)
    {
        var result = new Dictionary<string, List<LoggerCallNode>>();
        foreach (var node in callNodes)
        {
            var key = String.Join("", node.SplitLogText()); //"|"
            if (!result.ContainsKey(key))
                result[key] = new List<LoggerCallNode>();
            result[key].Add(node);
        }

        return result;
    }

    public IEnumerable<LoggerCallNode> GetPotentialCalls(string code, string logFragment)
    {
        var currentCodeCalls = GetLoggingNodes(code);

        return currentCodeCalls.Where(e =>
        {
            string placeholderPatternOR = "{...}";
            string placeholderPattern = "\\{\\.\\.\\.\\}";
            string pattern =
                $"(^|{placeholderPatternOR})(\\s*){Regex.Escape(logFragment)}(\\s*)({placeholderPatternOR}|$)";
            string newPattern = "^";
            foreach (var word in logFragment.Split(" "))
            {
                newPattern += $"({word}|{placeholderPattern})? ";
            }

            newPattern += "$";

            string newPattern2 = "";
            if (e.LogText.Contains("{...}"))
                newPattern2 = "^" + e.LogText.Substring(0, e.LogText.IndexOf("{...}", StringComparison.Ordinal)) +
                              ".*$";

            if (e.LogText.Equals(logFragment))
                return true;
            if (e.LogText.Equals("{...}"))
                return true;
            if (Regex.IsMatch(e.LogText, pattern))
                return true;
            if (Regex.IsMatch(e.LogText, newPattern))
                return true;
            if (Regex.IsMatch(logFragment, newPattern2))
                return true;
            return false;
        });
    }

    public static int FindLineIndexOfLog(string[] lines, string logLine) //TODO couldn't be static refactor it
    {
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains(logLine))
                return i;
        }

        return -1;
    }

    public class ClassBlock
    {
        public string Name { get; set; }
        public string Content { get; set; }
    }

    public static List<ClassBlock> ExtractClassBlocks(string code) //TODO couldn't be static refactor it
    {
        var result = new List<ClassBlock>();

        // Textual brace counting breaks on C# 12 body-less declarations ("class Foo : Bar;"),
        // on braces inside string literals/comments and on generic type names, so the file is
        // sliced with Roslyn - the same parser the analyzers already apply to each block.
        var root = CSharpSyntaxTree.ParseText(code).GetRoot();

        foreach (var typeDeclaration in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            // Parity with the previous regex: class/record/struct only, interfaces are skipped.
            if (typeDeclaration is InterfaceDeclarationSyntax)
                continue;

            result.Add(new ClassBlock
            {
                Name = typeDeclaration.Identifier.Text,
                Content = typeDeclaration.ToString()
            });
        }

        return result;
    }

    public IEnumerable<KeyValuePair<string, List<LoggerCallNode>>> GetPotentialCallsByPatterns(string code,
        string logFragment)
    {
        var currentCodeCalls = GetPatternsHashMap(
            GetLoggingNodes(code)
        );
        return currentCodeCalls.Where(e =>
            MatchTemplate(e.Key, logFragment)
        );
    }

    private string ReplaceAngleWithBraces(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return Regex.Replace(input, "<(.*?)>", "{$1}");
    }

    private string ReplaceWithEllipsis(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return Regex.Replace(input, "<.*?>", "{...}");
    }

    public static bool MatchTemplate(string template, string input)
    {
        if (template == null || input == null)
            return false;

        // Экранируем все спецсимволы, кроме < и >
        string escapedTemplate = Regex.Escape(template);

        // Заменяем экранированные <...> на паттерн .*?
        string pattern = Regex.Replace(template, @"<.*?>", ".*?");

        // Добавим ^ и $ чтобы вся строка подходила, а не только часть
        pattern = "^" + pattern + "$";

        return Regex.IsMatch(input, pattern);
    }

    #region Obsolete

    [Obsolete]
    private void StartAndPrint(string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();

        var classCommentTree = new List<ClassCommentNode>();

        foreach (var classNode in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            var classComments = GetLeadingComments(classNode);
            var methodNodes = classNode.DescendantNodes().OfType<MethodDeclarationSyntax>();

            var methodCommentNodes = new List<MethodCommentNode>();
            foreach (var methodNode in methodNodes)
            {
                var methodComments = GetLeadingComments(methodNode);

                methodCommentNodes.Add(new MethodCommentNode
                {
                    MethodName = methodNode.Identifier.Text,
                    Comments = methodComments
                });
            }

            classCommentTree.Add(new ClassCommentNode
            {
                ClassName = classNode.Identifier.Text,
                ClassComments = classComments,
                Methods = methodCommentNodes
            });
        }

        LogsAnalyzerViewBuilder.PrintCommentTree(classCommentTree);
    }
    
    [Obsolete]
    private List<string> GetLeadingComments(SyntaxNode node)
    {
        var trivia = node.GetLeadingTrivia();
        return trivia
            .Where(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia) || t.IsKind(SyntaxKind.MultiLineCommentTrivia) ||
                        t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
            .Select(t => t.ToString().Trim())
            .ToList();
    }

    #endregion

    public List<LoggerCallNode> Analyse(string plainText)
    {
        return (List<LoggerCallNode>)GetPotentialCalls(plainText, _searchText);
    }
}