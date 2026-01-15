namespace ProjectsScanner.Infrastructure;

/// <summary>
/// Core logic for data analysis
/// </summary>
/// <typeparam name="TOut">Output type</typeparam>
public interface IAnalyzer<out TOut>
{
    TOut Analyse(string plainText);
}