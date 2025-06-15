using System.Text.Json;

namespace rubberduckvba.Server.Model.Entity;

public record class FeatureEntity : Entity
{
    public int? ParentId { get; init; }
    public string FeatureName { get; init; } = default!;
    public int RepositoryId { get; init; }
    public string Title { get; init; } = default!;
    public string ShortDescription { get; init; } = default!;
    public string Description { get; init; } = default!;
    public bool IsHidden { get; init; }
    public bool IsNew { get; init; }
    public bool HasImage { get; init; }

    public string Links { get; init; } = string.Empty;
    public BlogLink[] BlogLinks => JsonSerializer.Deserialize<BlogLink[]>(Links) ?? [];
}

public record class BlogLink(string Name, string Url, string Author, string Published) { }

public record class AuditEntity
{
    public int Id { get; init; }
    public DateTime DateInserted { get; init; }
    public DateTime? DateModified { get; init; }
    public string Author { get; init; } = string.Empty;
}

public record class FeatureEditEntity : AuditEntity
{
    public int FeatureId { get; init; }
    public string FieldName { get; init; } = string.Empty;
    public string? ValueBefore { get; init; }
    public string ValueAfter { get; init; } = string.Empty;
    public string? ApprovedBy { get; init; }
    public DateTime? ApprovedAt { get; init; }
    public string? RejectedBy { get; init; }
    public DateTime? RejectedAt { get; init; }
}

public enum FeatureOperation
{
    Create = 1,
    Delete = 2,
}

public record class FeatureOpEntity : AuditEntity
{
    public FeatureOperation FeatureAction { get; init; }

    public int? ParentId { get; init; }
    public string FeatureName { get; init; } = default!;
    public int RepositoryId { get; init; }
    public string Title { get; init; } = default!;
    public string ShortDescription { get; init; } = default!;
    public string Description { get; init; } = default!;
    public bool IsHidden { get; init; }
    public bool IsNew { get; init; }
    public bool HasImage { get; init; }

    public string Links { get; init; } = string.Empty;
}