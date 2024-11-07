namespace rubberduckvba.Server.Model.Entity;

public record class FeatureEntity : Entity
{
    public int? ParentId { get; init; }
    public int RepositoryId { get; init; }
    public string Title { get; init; } = default!;
    public string ShortDescription { get; init; } = default!;
    public string Description { get; init; } = default!;
    public bool IsHidden { get; init; }
    public bool IsNew { get; init; }
    public bool HasImage { get; init; }
}
