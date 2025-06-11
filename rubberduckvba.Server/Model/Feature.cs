using rubberduckvba.Server.Model.Entity;
using System.Text.Json;

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

    //BlogLink[] Links { get; init; }

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
        FeatureName = entity.FeatureName;
        RepositoryId = (Services.RepositoryId)entity.RepositoryId;
        Title = entity.Title;
        ShortDescription = entity.ShortDescription;
        Description = entity.Description;
        IsHidden = entity.IsHidden;
        IsNew = entity.IsNew;
        HasImage = entity.HasImage;
        Links = string.IsNullOrWhiteSpace(entity.Links) ? []
            : JsonSerializer.Deserialize<BlogLink[]>(entity.Links, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
    }

    public int Id { get; init; }
    public DateTime DateTimeInserted { get; init; }
    public DateTime? DateTimeUpdated { get; init; }
    public string Name { get; init; } = string.Empty;

    public int? ParentId { get; init; }
    public string FeatureName { get; init; } = string.Empty;
    public Services.RepositoryId RepositoryId { get; init; } = Services.RepositoryId.Rubberduck;
    public string Title { get; init; } = string.Empty;
    public string ShortDescription { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsHidden { get; init; }
    public bool IsNew { get; init; }
    public bool IsDiscontinued { get; init; }
    public bool HasImage { get; init; }

    public BlogLink[] Links { get; init; } = [];

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
        FeatureName = FeatureName,
        RepositoryId = (int)Services.RepositoryId.Rubberduck,
        Title = Title,
        Links = Links.Length == 0 ? string.Empty : JsonSerializer.Serialize(Links)
    };

    public int GetContentHash()
    {
        var hash = new HashCode();
        hash.Add(Name);
        hash.Add(Title);
        hash.Add(ShortDescription);
        hash.Add(Description);
        hash.Add(HasImage);
        hash.Add(IsHidden);
        hash.Add(IsNew);
        hash.Add(IsDiscontinued);
        hash.Add(Links);
        return hash.ToHashCode();
    }
}

public record class FeatureGraph : Feature
{
    public FeatureGraph(FeatureEntity entity) : base(entity) { }

    public IEnumerable<Feature> Features { get; init; } = [];
    public IEnumerable<Inspection> Inspections { get; init; } = [];
    public IEnumerable<QuickFix> QuickFixes { get; init; } = [];
    public IEnumerable<Annotation> Annotations { get; init; } = [];
}