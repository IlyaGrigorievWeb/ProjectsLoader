using System.Text;
using Xunit.Abstractions;

namespace ProjectsScannetTests.Framework;

public class IOFilesFixture : IDisposable
{
    private Dictionary<string, string[]> _classesWithContent;
    
    public IOFilesFixture()
    {
        string sourceDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Features");
        var directories = Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories);
        
        _classesWithContent = new Dictionary<string, string[]>();
        foreach (var directory in directories)
        {
            var filePath = Directory.GetFiles(directory, "*.cs.ignore").Single();
            _classesWithContent[Path.GetFileNameWithoutExtension(filePath)] = File.ReadAllLines(filePath);
        }
    }

    public string GetFileContent(string suiteName)
    {
        return _classesWithContent[suiteName].Aggregate(new StringBuilder(),
                (sb, l) => sb.AppendLine(l),
                sb => sb.ToString().Trim());
    }

    public void Dispose()
    {
        
    }
}