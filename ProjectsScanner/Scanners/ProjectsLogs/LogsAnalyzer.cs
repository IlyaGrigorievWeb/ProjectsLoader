using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace ProjectsScanner.Scanners.ProjectsLogs;

public interface IAnalyzer
{
    public void StartAnalyze();
}

/*
If we see [].log[]("test") -> save text to call node 
*/

public class LogsAnalyzer : IAnalyzer
{
    //TODO work on it
    private SyntaxNode _endpointNode = null;
    // public LogsAnalyzer(SyntaxNode endpointNode)
    // {
    //     _endpointNode = endpointNode;
    // }

    private string _code;
    public LogsAnalyzer(string code)
    {
        _code = code;
    }
    
    
    //----------------------------------------------------------------------
    public void StartAnalyze()
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

        PrintCommentTree(classCommentTree);
    }
    static List<string> GetLeadingComments(SyntaxNode node)
    {
        var trivia = node.GetLeadingTrivia();
        return trivia
            .Where(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia) || t.IsKind(SyntaxKind.MultiLineCommentTrivia) ||
                        t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
            .Select(t => t.ToString().Trim())
            .ToList();
    }
    static void PrintCommentTree(List<ClassCommentNode> classCommentTree)
    {
        foreach (var classNode in classCommentTree)
        {
            Console.WriteLine($"Class: {classNode.ClassName}");

            if (classNode.ClassComments.Any())
            {
                Console.WriteLine("  Comments:");
                foreach (var comment in classNode.ClassComments)
                {
                    Console.WriteLine($"    {comment}");
                }
            }

            foreach (var methodNode in classNode.Methods)
            {
                Console.WriteLine($"  Method: {methodNode.MethodName}");
                if (methodNode.Comments.Any())
                {
                    Console.WriteLine("    Comments:");
                    foreach (var comment in methodNode.Comments)
                    {
                        Console.WriteLine($"      {comment}");
                    }
                }
            }
        }
    }
    //----------------------------------------------------------------------

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
                    if (memberAccess != null && memberAccess.Name.Identifier.Text.Contains("log", StringComparison.OrdinalIgnoreCase))
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
                            var overArguments = new List<Tuple<string, string>>();
                            //Save arguments except 0 index (only arguments)
                            if (argumentList.Count > 1)
                            {
                                for (int i = 0; i < argumentList.Count - 1; i++)
                                {
                                    if (i != 0)
                                    {
                                        overArguments.Add(Tuple.Create<string, string>(
                                            "",//TODO: not implemented reading type of arguments argumentList[i].Expression.GetMemberType(),
                                            argumentList[i].GetText().ToString()
                                        ));
                                    }
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
                                        Parameters = overArguments
                                    });
                            }
                        }
                    }
                }
            }
            
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
    
    static int CalculateLevenshteinDistance(string source, string target)
    {
        int[,] dp = new int[source.Length + 1, target.Length + 1];

        for (int i = 0; i <= source.Length; i++)
            dp[i, 0] = i;
        for (int j = 0; j <= target.Length; j++)
            dp[0, j] = j;

        for (int i = 1; i <= source.Length; i++)
        {
            for (int j = 1; j <= target.Length; j++)
            {
                int cost = source[i - 1] == target[j - 1] ? 0 : 1;

                dp[i, j] = Math.Min(
                    Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                    dp[i - 1, j - 1] + cost);
            }
        }

        return dp[source.Length, target.Length];
    }
    
    public static string FindBestMatchPosition(string original, string query)
    {
        var mlContext = new MLContext();

        string[] words = original.Split(' ');
        List<string> subTexts = new List<string>();

        // Generate combinations of input text
        for (int i = 0; i < words.Length; i++)
        {
            for (int length = 1; length <= words.Length - i; length++)
            {
                subTexts.Add(string.Join(" ", words.Skip(i).Take(length)));
            }
        }

        // Initialize data-set
        var data = subTexts.Select(text => new InputData { Text = text }).ToList();
        var trainData = mlContext.Data.LoadFromEnumerable(data);

        // Create pipeline with TF-IDF
        var pipeline = mlContext.Transforms.Text.FeaturizeText("Features", nameof(InputData.Text));
        var model = pipeline.Fit(trainData);

        // Transform data
        var transformedData = model.Transform(trainData);
        var featureData = mlContext.Data.CreateEnumerable<TransformedData>(transformedData, reuseRowObject: false).ToList();

        // Vectorized query
        var queryData = mlContext.Data.LoadFromEnumerable(new List<InputData> { new InputData { Text = query } });
        var queryTransformed = model.Transform(queryData);
        var queryFeature = mlContext.Data.CreateEnumerable<TransformedData>(queryTransformed, reuseRowObject: false).First().Features;

        // Search nearest result
        int bestIndex = -1;
        double bestSimilarity = -1;

        for (int i = 0; i < featureData.Count; i++)
        {
            double similarity = CosineSimilarity(featureData[i].Features, queryFeature);
            if (similarity > bestSimilarity)
            {
                bestSimilarity = similarity;
                bestIndex = i;
            }
        }

        return subTexts[bestIndex];
        //return FindOriginalIndex(original, subTexts[bestIndex]);
    }

    static int FindOriginalIndex(string original, string subText)
    {
        var words = original.Split(' ');
        var subWords = subText.Split(' ');

        for (int i = 0; i <= words.Length - subWords.Length; i++)
        {
            if (string.Join(" ", words.Skip(i).Take(subWords.Length)) == subText)
            {
                return i;
            }
        }

        return -1;
    }

    static double CosineSimilarity(float[] vectorA, float[] vectorB)
    {
        double dotProduct = 0;
        double magnitudeA = 0;
        double magnitudeB = 0;

        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            magnitudeA += Math.Pow(vectorA[i], 2);
            magnitudeB += Math.Pow(vectorB[i], 2);
        }

        magnitudeA = Math.Sqrt(magnitudeA);
        magnitudeB = Math.Sqrt(magnitudeB);

        return (magnitudeA == 0 || magnitudeB == 0) ? 0 : dotProduct / (magnitudeA * magnitudeB);
    }

    public class InputData { public string Text { get; set; } }
    public class TransformedData : InputData { public float[] Features { get; set; } }
    
    //TODO add view builder as separate class
    public static string GetView(List<LoggerCallNode> logNodes)
    {
        var stringResult = "";
        
        var groupedLogs = logNodes
            .GroupBy(node => node.ClassName)
            .Select(classGroup => new
            {
                ClassName = classGroup.Key,
                Methods = classGroup
                    .GroupBy(node => node.MethodName)
                    .Select(methodGroup => new
                    {
                        MethodName = methodGroup.Key,
                        LogTexts = methodGroup.Select(node => node.LogText).ToList()
                    }).ToList()
            });

        foreach (var classGroup in groupedLogs)
        {
            stringResult += $"\n{classGroup.ClassName} :";
            foreach (var methodGroup in classGroup.Methods)
            {
                stringResult += $"\n    {methodGroup.MethodName} :";
                stringResult += $"\n      {string.Join(",\n      ", methodGroup.LogTexts)}";
            }
        }
        
        return stringResult;
    }

    public static SyntaxNode buildParentSyntaxNode(string projectPath, string endpointClassName)
    {
        throw new NotImplementedException();
    }
    
    public static SyntaxNode buildParentSyntaxNode(string classPath)
    {
        throw new NotImplementedException();
    }
}

// public class SyntaxNode
// {
//     SyntaxNodeType _nodeType;
//     string _content;
//     List<SyntaxNode> _children;
//
//     public SyntaxNode(List<SyntaxNode> children, string content, SyntaxNodeType nodeType)
//     {
//         _children = children;
//         _content = content;
//         _nodeType = nodeType;
//     }
// }

public enum SyntaxNodeType
{
    Class,
    Constructor,
    Infrasctructure,
    Method,
    StaticMethod,
    Condition,
    Cycle
}

public class LoggerCallNode
{
    public string ClassName { get; set; }
    
    public string MethodName { get; set; }
    
    public string LogText { get; set; }

    public List<Tuple<string, string>> Parameters { get; set; }
}

public class ClassCommentNode
{
    public string ClassName { get; set; }
    public List<string> ClassComments { get; set; } = new();
    public List<MethodCommentNode> Methods { get; set; } = new();
}

public class MethodCommentNode
{
    public string MethodName { get; set; }
    public List<string> Comments { get; set; } = new();
}