namespace ProjectsScanner.Scanners;

public interface ICodeScanner : IProjectScanner
{
    bool isLoggingSupported();

    string getLoggerFramework();
}