namespace rubberduckvba.Server.Model.Entity;

public record class InspectionEntity : Entity
{
    public int FeatureId { get; init; }
    public int TagAssetId { get; init; }
    public string SourceUrl { get; init; } = string.Empty;
    public string InspectionType { get; init; } = string.Empty;
    public string DefaultSeverity { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string Reasoning { get; init; } = string.Empty;
    public string? Remarks { get; init; }
    public string? HostApp { get; init; }
    public bool IsNew { get; init; }
    public bool IsDiscontinued { get; init; }
    public bool IsHidden { get; init; }
    public string? References { get; init; }
    public string? QuickFixes { get; init; }
    public string? JsonExamples { get; init; }
}
