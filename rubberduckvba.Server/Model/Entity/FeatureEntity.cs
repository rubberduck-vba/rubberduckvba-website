using System.Text.Json;

namespace rubberduckvba.Server.Model.Entity;

public record class FeatureEntity : Entity
{
    public int? FeatureId { get; init; }
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
