using System.Runtime.Serialization;
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

    public string? ApprovedBy { get; init; }
    public DateTime? ApprovedAt { get; init; }
    public string? RejectedBy { get; init; }
    public DateTime? RejectedAt { get; init; }

    public bool IsPending => !ApprovedAt.HasValue && !RejectedAt.HasValue;
    public bool IsApproved => ApprovedAt.HasValue && !RejectedAt.HasValue;
}

public enum AuditActivityType
{
    [EnumMember(Value = nameof(SubmitEdit))]
    SubmitEdit,
    [EnumMember(Value = nameof(ApproveEdit))]
    ApproveEdit,
    [EnumMember(Value = nameof(RejectEdit))]
    RejectEdit,
    [EnumMember(Value = nameof(SubmitCreate))]
    SubmitCreate,
    [EnumMember(Value = nameof(ApproveCreate))]
    ApproveCreate,
    [EnumMember(Value = nameof(RejectCreate))]
    RejectCreate,
    [EnumMember(Value = nameof(SubmitDelete))]
    SubmitDelete,
    [EnumMember(Value = nameof(ApproveDelete))]
    ApproveDelete,
    [EnumMember(Value = nameof(RejectDelete))]
    RejectDelete,
}

public record class AuditActivityEntity
{
    public int Id { get; init; }
    public string Author { get; init; } = string.Empty;
    public DateTime ActivityTimestamp { get; init; }
    public string Activity { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? ReviewedBy { get; init; }
}

public record class FeatureEditEntity : AuditEntity
{
    public int FeatureId { get; init; }
    public string FieldName { get; init; } = string.Empty;
    public string? ValueBefore { get; init; }
    public string ValueAfter { get; init; } = string.Empty;
}

public record class FeatureEditViewEntity : FeatureEditEntity
{
    public string FeatureName { get; init; } = string.Empty;
    //public bool IsStale { get; init; }
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
    public string Name { get; init; } = default!;
    public string Title { get; init; } = default!;
    public string ShortDescription { get; init; } = default!;
    public string Description { get; init; } = default!;
    public bool IsHidden { get; init; }
    public bool IsNew { get; init; }
    public bool HasImage { get; init; }

    public string Links { get; init; } = string.Empty;
}