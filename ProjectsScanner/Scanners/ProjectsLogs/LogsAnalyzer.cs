using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace ProjectsScanner.Scanners.ProjectsLogs;

//TODO Need to support different endpoints: project file path based, SyntaxNode based and one file (class) based 
public class LogsAnalyzer
{
    //Tried to use ML MS libs and levenshtein distance. Regex is an optimal way for MVP of "LogsAnalyzer"
    private SyntaxNode _endpointNode = null; //TODO Better to use Roslyn tree here
    private string _code;
    public LogsAnalyzer(string code)
    {
        _code = code;
    }
    
    public void StartAndPrint()
    {
        var tree = CSharpSyntaxTree.ParseText(_code);
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
    List<string> GetLeadingComments(SyntaxNode node)
    {
        var trivia = node.GetLeadingTrivia();
        return trivia
            .Where(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia) || t.IsKind(SyntaxKind.MultiLineCommentTrivia) ||
                        t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
            .Select(t => t.ToString().Trim())
            .ToList();
    }
    
    public List<LoggerCallNode> GetLoggingNodes()
    {
        var tree = CSharpSyntaxTree.ParseText(_code);
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
                    if ((memberAccess != null && memberAccess.Name.Identifier.Text.Contains("log", StringComparison.OrdinalIgnoreCase))
                        || invocation.Parent!.GetText().ToString().Trim().StartsWith("log", StringComparison.OrdinalIgnoreCase))
                    {
                        // Extract the argument passed to the logging call
                        var argumentList = invocation.ArgumentList.Arguments;
                        if (argumentList.Count > 0)
                        {
                            var argument = argumentList[0].Expression;
                            // Attempt to evaluate the argument to get the string value
                            string logMessage = null;

                            if (argument is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
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
                            else if (argument is LiteralExpressionSyntax verbatimLiteral && verbatimLiteral.IsKind(SyntaxKind.StringLiteralExpression))
                            {
                                // Handle verbatim strings (@"...")
                                logMessage = verbatimLiteral.Token.ValueText;
                            }

                            //Saving over arguments
                            var overArguments = new Dictionary<string, string>();
                            var argumentsInOrder = new string[argumentList.Count-1];
                            //Save arguments except 0 index (only arguments)
                            if (argumentList.Count > 1)
                            {
                                for (int i = 1; i < argumentList.Count; i++)
                                {
                                    //TODO: not implemented reading type of arguments argumentList[i].Expression.GetMemberType()
                                    overArguments[argumentList[i].GetText().ToString()] = "UnknownType";
                                    argumentsInOrder[i-1] = argumentList[i].GetText().ToString();
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
            var key = String.Join("|", node.SplitLogText());
            if (!result.ContainsKey(key))
                result[key] = new List<LoggerCallNode>();
            result[key].Add(node);
        }

        return result;
    }

    public IEnumerable<LoggerCallNode> GetPotentialCalls(string logFragment)
    {
        var currentCodeCalls = GetLoggingNodes();

        return currentCodeCalls.Where(e =>
        {
            string placeholderPatternOR = "{...}";
            string placeholderPattern = "\\{\\.\\.\\.\\}";
            string pattern = $"(^|{placeholderPatternOR})(\\s*){Regex.Escape(logFragment)}(\\s*)({placeholderPatternOR}|$)";
            string newPattern = "^";
            foreach (var word in logFragment.Split(" "))
            {
                newPattern += $"({word}|{placeholderPattern})? ";
            }
            newPattern += "$";

            string newPattern2 = "";
            if(e.LogText.Contains("{...}"))
                newPattern2 = "^" + e.LogText.Substring(0, e.LogText.IndexOf("{...}", StringComparison.Ordinal)) + ".*$";
            
            if (e.LogText.Equals(logFragment))
                return true;
            else if (e.LogText.Equals("{...}"))
                return true;
            else if(Regex.IsMatch(e.LogText, pattern))
                return true;
            else if(Regex.IsMatch(e.LogText, newPattern))
                return true;
            else if(Regex.IsMatch(logFragment, newPattern2 ))
                return true;
            return false;
        });
    }
}