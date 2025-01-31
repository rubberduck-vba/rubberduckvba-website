using rubberduckvba.Server.Model.Entity;
using System.Text.Json;

namespace rubberduckvba.Server.Model;

public record class Inspection() : IFeature
{
    public Inspection(InspectionEntity entity) : this()
    {
        Id = entity.Id;
        DateTimeInserted = entity.DateTimeInserted;
        DateTimeUpdated = entity.DateTimeUpdated;
        TagAssetId = entity.TagAssetId;
        SourceUrl = entity.SourceUrl;

        FeatureId = entity.FeatureId;
        FeatureName = entity.FeatureName;

        IsNew = entity.IsNew;
        IsDiscontinued = entity.IsDiscontinued;
        IsHidden = entity.IsHidden;

        Name = entity.Name;
        InspectionType = entity.InspectionType;
        DefaultSeverity = entity.DefaultSeverity;
        Summary = entity.Summary;
        Reasoning = entity.Reasoning;
        Remarks = entity.Remarks;
        HostApp = entity.HostApp;
        References = entity.References?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];
        QuickFixes = entity.QuickFixes?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];
        Examples = entity.JsonExamples is null ? [] : JsonSerializer.Deserialize<InspectionExample[]>(entity.JsonExamples) ?? [];
    }

    public int Id { get; init; }
    public DateTime DateTimeInserted { get; init; }
    public DateTime? DateTimeUpdated { get; init; }
    public int TagAssetId { get; init; }
    public string SourceUrl { get; init; } = string.Empty;

    public int FeatureId { get; init; }
    public string FeatureName { get; init; }

    public bool IsNew { get; init; }
    public bool IsDiscontinued { get; init; }
    public bool IsHidden { get; init; }

    public string Name { get; init; } = string.Empty;
    public string InspectionType { get; init; } = string.Empty;
    public string DefaultSeverity { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string Reasoning { get; init; } = string.Empty;
    public string? Remarks { get; init; }
    public string? HostApp { get; init; }
    public string[] References { get; init; } = [];
    public string[] QuickFixes { get; init; } = [];
    public InspectionExample[] Examples { get; init; } = [];

    public InspectionEntity ToEntity() => new()
    {
        Id = Id,
        FeatureId = FeatureId,
        FeatureName = FeatureName,
        DateTimeInserted = DateTimeInserted,
        DateTimeUpdated = DateTimeUpdated,
        TagAssetId = TagAssetId,
        SourceUrl = SourceUrl,
        IsNew = IsNew,
        IsDiscontinued = IsDiscontinued,
        IsHidden = IsHidden,
        Name = Name,
        InspectionType = InspectionType,
        DefaultSeverity = DefaultSeverity,
        Summary = Summary,
        Reasoning = Reasoning,
        Remarks = Remarks,
        HostApp = HostApp,
        JsonExamples = JsonSerializer.Serialize(Examples),
        QuickFixes = string.Join(',', QuickFixes),
        References = string.Join(',', References),
    };

    public int GetContentHash()
    {
        var hash = new HashCode();
        hash.Add(DefaultSeverity);
        hash.Add(Summary);
        hash.Add(Reasoning);
        hash.Add(Remarks);
        hash.Add(HostApp);
        hash.Add(Name);
        hash.Add(InspectionType);
        hash.Add(JsonSerializer.Serialize(Examples));
        hash.Add(string.Join(',', QuickFixes));
        hash.Add(string.Join(',', References));
        return hash.ToHashCode();
    }
}
