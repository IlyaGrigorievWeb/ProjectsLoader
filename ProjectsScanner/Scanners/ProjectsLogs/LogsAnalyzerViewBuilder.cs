namespace ProjectsScanner.Scanners.ProjectsLogs;

//TODO Now it's trash bin for view layer utils, need to refactor it and move to "client" app
public static class LogsAnalyzerViewBuilder
{
    public static void PrintCommentTree(List<ClassCommentNode> classCommentTree)
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
}