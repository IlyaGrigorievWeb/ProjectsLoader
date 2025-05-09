using System.Text.RegularExpressions;

namespace ProjectsScanner.Scanners.ProjectsLogs;

public class LoggerCallNode
{
    public string ClassName { get; set; }
    
    public string MethodName { get; set; }
    
    public string LogText { get; set; }

    public string[] ParametersInOrder { get; set; }

    //Name, Type
    public Dictionary<string, string> Parameters { get; set; }

    public string getFullInfoLogDescription()
    {
        string formattedLog = LogText;
        foreach (var parameter in Parameters)
        {
            string placeholder = $"{{{parameter.Key}}}";
            string replacement = $"<[{parameter.Key}:{parameter.Value}]>";
            formattedLog = formattedLog.Replace(placeholder, replacement);
        }
        return $"{ClassName} {MethodName} {formattedLog}";
    }
    
    public string[] SplitLogText()
    {
        List<string> parts = new List<string>();
        string pattern = "\\{(.*?)\\}";
        int lastIndex = 0;
        int varIndex = 0;
        
        foreach (Match match in Regex.Matches(LogText, pattern))
        {
            if (match.Index > lastIndex)
            {
                parts.Add(LogText.Substring(lastIndex, match.Index - lastIndex));
            }
            string paramName = match.Groups[1].Value;
            string paramValue = "";
            //TODO need to support $ and params view separatly. This code broken for $
            if (ParametersInOrder.Length != 0)
            {
                paramValue = ParametersInOrder[varIndex];
                parts.Add($"<{Parameters[paramValue]}>");
                varIndex++;
                lastIndex = match.Index + match.Length;
            }
        }
        
        if (lastIndex < LogText.Length)
        {
            parts.Add(LogText.Substring(lastIndex));
        }
        
        return parts.ToArray();
    }
}