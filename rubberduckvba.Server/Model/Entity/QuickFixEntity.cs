namespace rubberduckvba.Server.Model.Entity;

public record class QuickFixEntity : Entity
{
    public int FeatureId { get; init; }
    public string FeatureName { get; init; } = string.Empty;
    public int TagAssetId { get; init; }
    public string SourceUrl { get; init; } = string.Empty;
    public bool IsNew { get; init; }
    public bool IsDiscontinued { get; init; }
    public bool IsHidden { get; init; }
    public string Summary { get; init; } = string.Empty;
    public string? Remarks { get; init; }
    public bool CanFixMultiple { get; init; }
    public bool CanFixProcedure { get; init; }
    public bool CanFixModule { get; init; }
    public bool CanFixProject { get; init; }
    public bool CanFixAll { get; init; }
    public string Inspections { get; init; } = string.Empty;
    public string? JsonExamples { get; init; }
}
