using rubberduckvba.Server.Model.Entity;
using System.Text.Json;

namespace rubberduckvba.Server.Model;

public record class QuickFix()
{
    public QuickFix(QuickFixEntity entity) : this()
    {
        Id = entity.Id;
        DateTimeInserted = entity.DateTimeInserted;
        DateTimeUpdated = entity.DateTimeUpdated;
        FeatureId = entity.FeatureId;
        TagAssetId = entity.TagAssetId;
        SourceUrl = entity.SourceUrl;
        IsNew = entity.IsNew;
        IsDiscontinued = entity.IsDiscontinued;
        IsHidden = entity.IsHidden;
        Name = entity.Name;
        Summary = entity.Summary;
        Remarks = entity.Remarks;
        CanFixMultiple = entity.CanFixMultiple;
        CanFixProcedure = entity.CanFixProcedure;
        CanFixModule = entity.CanFixModule;
        CanFixProject = entity.CanFixProject;
        CanFixAll = entity.CanFixAll;
        Inspections = entity.Inspections.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];
        Examples = entity.JsonExamples is null ? [] : JsonSerializer.Deserialize<QuickFixExample[]>(entity.JsonExamples) ?? [];
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
    public string? Remarks { get; init; }
    public bool CanFixMultiple { get; init; }
    public bool CanFixProcedure { get; init; }
    public bool CanFixModule { get; init; }
    public bool CanFixProject { get; init; }
    public bool CanFixAll { get; init; }
    public string[] Inspections { get; init; } = [];
    public QuickFixExample[] Examples { get; init; } = [];

    public QuickFixEntity ToEntity() => new()
    {
        Id = Id,
        DateTimeInserted = DateTimeInserted,
        DateTimeUpdated = DateTimeUpdated,
        FeatureId = FeatureId,
        SourceUrl = SourceUrl,
        IsNew = IsNew,
        IsDiscontinued = IsDiscontinued,
        IsHidden = IsHidden,
        Name = Name,
        Summary = Summary,
        Remarks = Remarks,
        CanFixMultiple = CanFixMultiple,
        CanFixProcedure = CanFixProcedure,
        CanFixModule = CanFixModule,
        CanFixProject = CanFixProject,
        CanFixAll = CanFixAll,
        TagAssetId = TagAssetId,
        JsonExamples = JsonSerializer.Serialize(Examples),
        Inspections = string.Join(',', Inspections),
    };
}