using rubberduckvba.Server.Model.Entity;

namespace rubberduckvba.Server.Model;

public interface IFeature
{
    int Id { get; init; }
    DateTime DateTimeInserted { get; init; }
    DateTime? DateTimeUpdated { get; init; }
    string Name { get; init; }

    bool IsNew { get; init; }
    bool IsHidden { get; init; }
    bool IsDiscontinued { get; init; }

    int GetContentHash();
}

public record class Feature() : IFeature
{
    public Feature(FeatureEntity entity) : this()
    {
        Id = entity.Id;
        DateTimeInserted = entity.DateTimeInserted;
        DateTimeUpdated = entity.DateTimeUpdated;
        Name = entity.Name;
        ParentId = entity.ParentId;
        RepositoryId = (Services.RepositoryId)entity.RepositoryId;
        Title = entity.Title;
        ShortDescription = entity.ShortDescription;
        Description = entity.Description;
        IsHidden = entity.IsHidden;
        IsNew = entity.IsNew;
        HasImage = entity.HasImage;
    }

    public int Id { get; init; }
    public DateTime DateTimeInserted { get; init; }
    public DateTime? DateTimeUpdated { get; init; }
    public string Name { get; init; } = string.Empty;

    public int? ParentId { get; init; }
    public Services.RepositoryId RepositoryId { get; init; } = Services.RepositoryId.Rubberduck;
    public string Title { get; init; } = string.Empty;
    public string ShortDescription { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsHidden { get; init; }
    public bool IsNew { get; init; }
    public bool IsDiscontinued { get; init; }
    public bool HasImage { get; init; }

    public FeatureEntity ToEntity() => new()
    {
        Id = Id,
        DateTimeInserted = DateTimeInserted,
        DateTimeUpdated = DateTimeUpdated,
        Description = Description,
        HasImage = HasImage,
        IsHidden = IsHidden,
        IsNew = IsNew,
        Name = Name,
        ShortDescription = ShortDescription,
        ParentId = ParentId,
        RepositoryId = (int)Services.RepositoryId.Rubberduck,
        Title = Title,
    };

    public int GetContentHash() => HashCode.Combine(Name, Title, ShortDescription, Description, HasImage, IsHidden, IsNew, IsDiscontinued);
}

public record class FeatureGraph : Feature
{
    public FeatureGraph(FeatureEntity entity) : base(entity) { }

    public string? ParentName { get; init; }
    public string? ParentTitle { get; init; }

    public IEnumerable<Feature> Features { get; init; } = [];
    public IEnumerable<Inspection> Inspections { get; init; } = [];
    public IEnumerable<QuickFix> QuickFixes { get; init; } = [];
    public IEnumerable<Annotation> Annotations { get; init; } = [];
}