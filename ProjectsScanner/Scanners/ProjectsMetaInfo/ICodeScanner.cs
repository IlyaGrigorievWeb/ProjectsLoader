namespace ProjectsScanner.Scanners;

public interface ICodeScanner<T> : IProjectScanner
{
    T isLoggingSupported();

    T getLoggerFramework();
}