namespace ProjectsScannetTests.Framework;

public class FileTestBase
{
    protected string InputFileName { get;}

    protected FileTestBase()
    {
        string className = GetType().Name.Replace("Test", "");
        InputFileName = Path.Combine($"{className}.cs");
    }
}