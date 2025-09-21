using ProjectsScanner.Scanners;

namespace ProjectsLoader.Models;

/// <summary>
/// Mock model that captures common stats of a class file.
/// </summary>
public class ClassStats : IMergeableModel<ClassStats>
{
    public int FunctionCount { get; set; }
    public int PropertyCount { get; set; }

    public static ClassStats NewInstance() => new ClassStats();

    public void Merge(ClassStats model)
    {
        FunctionCount += model.FunctionCount;
        PropertyCount += model.PropertyCount;
    }
}