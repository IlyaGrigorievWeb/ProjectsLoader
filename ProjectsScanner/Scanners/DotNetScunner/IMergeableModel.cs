namespace ProjectsScanner.Scanners;

/// <summary>
/// Models implementing this interface have a property to be merged together.
/// Consider a task at hand when you analyze multiple files in parallel and produce some model, at
/// the end you'll need to merge it into 1 aggregating all the fields.
/// Or suppose you want to have set of running results and merge as the results come in.
///
/// Example usage:
/// <code>
///     var myModel: IMergeableModel = new TModel();
///     foreach (var node in project.GetNodes())
///     {
///         myModel.Merge(analyzer.Analyze(node));
///     }
/// </code>
/// </summary>
public interface IMergeableModel<in TModel>
{
    
    void Merge(TModel model);
}