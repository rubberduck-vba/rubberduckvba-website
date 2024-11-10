namespace rubberduckvba.Server.Model;

public record class Example
{
    public string Description { get; init; } = default!;
    public int SortOrder { get; init; }
}

public record class InspectionExample : Example
{
    /// <summary>
    /// <c>True</c> if the example depicts code a situation that would make an inspection issue a result.
    /// </summary>
    public bool HasResult { get; init; }
    public ICollection<ExampleModule> Modules { get; init; } = [];
}

public record class QuickFixExample : BeforeAndAfterExample
{
}

public record class AnnotationExample : BeforeAndAfterExample
{
    /// <summary>
    /// The example modules when the annotation has no before/after examples.
    /// </summary>
    public ICollection<ExampleModule> Modules { get; init; } = [];
}

public record class BeforeAndAfterExample : Example
{
    /// <summary>
    /// The code modules before feature is used.
    /// </summary>
    public ICollection<ExampleModule> ModulesBefore { get; init; } = [];
    /// <summary>
    /// The code modules after the feature is used.
    /// </summary>
    public ICollection<ExampleModule> ModulesAfter { get; init; } = [];
}
