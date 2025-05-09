namespace ProjectsScanner.Scanners.ProjectsLogs;

public class ClassCommentNode
{
    public string ClassName { get; set; }
    public List<string> ClassComments { get; set; } = new();
    public List<MethodCommentNode> Methods { get; set; } = new();
}