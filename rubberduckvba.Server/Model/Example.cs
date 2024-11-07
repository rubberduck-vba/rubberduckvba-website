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

public record class QuickFixExample : Example
{
    /// <summary>
    /// The code modules before the quickfix is applied.
    /// </summary>
    public ICollection<ExampleModule> ModulesBefore { get; init; } = [];
    /// <summary>
    /// The code modules after the quickfix is applied.
    /// </summary>
    public ICollection<ExampleModule> ModulesAfter { get; init; } = [];
}

public record class AnnotationExample : Example
{
    public ICollection<ExampleModule> Modules { get; init; } = [];
}
