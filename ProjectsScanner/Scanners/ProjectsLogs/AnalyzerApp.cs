using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ProjectsScanner.Scanners.ProjectsLogs;

public class AnalyzerApp
{
//     public async static void main()
//     {
//         var code = @"
//         public class ClassWithLogs
//         {
//             ILogger<ClassWithLogs> _logger;
//
//             public ClassWithLogs(ILogger<ClassWithLogs> logger)
//             {
//                 _logger = logger;
//             }
//             
//             public void Run()
//             {
//                 string a = ""variable"";
//                 
//                 _logger.LogInformation(""Hello World!"");
//                 
//                 _logger.LogInformation($""Hello {a}"");
//             }
//         }
//         ";
//
//         LogsAnalyzer logsAnalyzer = new LogsAnalyzer(code);
//         logsAnalyzer.StartAnalyze();
//         var commentsTree = logsAnalyzer.GetLoggingNodes();
//         Console.WriteLine(LogsAnalyzer.GetView(commentsTree));
//         
//     }
    public static void main()
    {
        string row = "abc qwe";
        List<string> inputCalls = new List<string>
        {
            "abc",
            "abc{...}",
            "abc {...}",
            "{...}abc{...}",
            "{...}",
            "abc {...}",
            "{...} qwe",
            "{...}",
            "abc qwe",
            "abcqwe",
            "qwe",
            "abc",
            "asd"
        };
        
        var result = FindMatchingStrings(row, inputCalls);
        
        Console.WriteLine("{");
        foreach (var str in result)
        {
            Console.WriteLine("\"" + str + "\",");
        }
        Console.WriteLine("}");
    }
    
    static List<string> FindMatchingStrings(string row, List<string> inputCalls)
    {
        string placeholderPattern = "{...}";
        string pattern = $"(^|{placeholderPattern})(\\s*){Regex.Escape(row)}(\\s*)({placeholderPattern}|$)";
        string newPattern = "^";
        foreach (var word in row.Split(" "))
        {
            newPattern += $"({word}|{placeholderPattern})? ";
        }
        newPattern += "$";
        var newPattern2 = Regex.Replace(newPattern, placeholderPattern, pattern);
        var res1 = inputCalls.Where(s => Regex.IsMatch(s, pattern)).ToList();
        var res2 = inputCalls.Where(s => Regex.IsMatch(s + " ", newPattern)).ToList();
        var res3 = inputCalls.Where(s => Regex.IsMatch(s, placeholderPattern)).ToList();
        var res4 = inputCalls.Where(s => Regex.IsMatch(s, newPattern)).ToList();
        return res4;
    }
}