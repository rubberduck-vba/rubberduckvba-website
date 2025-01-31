namespace rubberduckvba.Server.Model.Entity;

public record class AnnotationEntity : Entity
{
    public int FeatureId { get; init; }
    public string FeatureName { get; init; } = string.Empty;
    public int TagAssetId { get; init; }
    public string SourceUrl { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string Remarks { get; init; } = string.Empty;
    public bool IsNew { get; init; }
    public bool IsDiscontinued { get; init; }
    public bool IsHidden { get; init; }
    public string? JsonParameters { get; init; }
    public string? JsonExamples { get; init; }
}
