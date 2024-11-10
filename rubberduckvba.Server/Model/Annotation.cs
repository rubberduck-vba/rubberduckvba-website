using rubberduckvba.Server.Model.Entity;
using System.Text.Json;

namespace rubberduckvba.Server.Model;

public record class Annotation() : IFeature
{
    public Annotation(AnnotationEntity entity) : this()
    {
        Id = entity.Id;
        DateTimeInserted = entity.DateTimeInserted;
        DateTimeUpdated = entity.DateTimeUpdated;
        FeatureId = entity.FeatureId;
        TagAssetId = entity.TagAssetId;
        SourceUrl = entity.SourceUrl;
        Name = entity.Name;
        Parameters = entity.JsonParameters is null ? [] : JsonSerializer.Deserialize<AnnotationParameter[]>(entity.JsonParameters) ?? [];
        Examples = entity.JsonExamples is null ? [] : JsonSerializer.Deserialize<AnnotationExample[]>(entity.JsonExamples) ?? [];
    }

    public int Id { get; init; }
    public DateTime DateTimeInserted { get; init; }
    public DateTime? DateTimeUpdated { get; init; }
    public int FeatureId { get; init; }
    public int TagAssetId { get; init; }
    public string SourceUrl { get; init; } = string.Empty;
    public bool IsNew { get; init; }
    public bool IsDiscontinued { get; init; }
    public bool IsHidden { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string Remarks { get; init; } = string.Empty;
    public AnnotationParameter[] Parameters { get; init; } = [];
    public AnnotationExample[] Examples { get; init; } = [];

    internal AnnotationEntity ToEntity() => new()
    {
        Id = Id,
        DateTimeInserted = DateTimeInserted,
        DateTimeUpdated = DateTimeUpdated,
        FeatureId = FeatureId,
        TagAssetId = TagAssetId,
        SourceUrl = SourceUrl,
        Name = Name,
        Summary = Summary,
        Remarks = Remarks,
        JsonParameters = JsonSerializer.Serialize(Parameters),
        JsonExamples = JsonSerializer.Serialize(Examples),
    };

    public int GetContentHash()
    {
        var hash = new HashCode();
        hash.Add(Name);
        hash.Add(Summary);
        hash.Add(Remarks);
        hash.Add(JsonSerializer.Serialize(Parameters));
        hash.Add(JsonSerializer.Serialize(Examples));
        return hash.ToHashCode();
    }
}
