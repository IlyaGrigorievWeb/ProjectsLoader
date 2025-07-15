namespace ProjectsScanner.Scanners.ProjectsLogs;

public class MethodCommentNode
{
    public string MethodName { get; set; }
    public List<string> Comments { get; set; } = new();
}