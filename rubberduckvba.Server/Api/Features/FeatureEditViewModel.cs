using rubberduckvba.Server.Model;
using rubberduckvba.Server.Services;

namespace rubberduckvba.Server.Api.Features;

public class FeatureEditViewModel
{
    public FeatureEditViewModel()
    {

    }

    public static FeatureEditViewModel Default(RepositoryId repository, FeatureOptionViewModel? parent, IEnumerable<FeatureOptionViewModel> features, RepositoryOptionViewModel[] repositories) =>
        new()
        {
            Features = features.ToArray(),
            Repositories = repositories,

            RepositoryId = repository,
            ParentId = parent?.Id,
            Name = parent is null ? "NewFeature" : $"New{parent.Name}Feature",
            Title = "Feature Title",
            ShortDescription = "A short description; markdown is supported.",
            Description = "A markdown document with the feature's (user) documentation."
        };

    public Feature ToFeature()
    {
        return new Feature
        {
            Id = Id ?? default,
            FeatureId = ParentId,
            RepositoryId = RepositoryId,
            Name = Name,
            Title = Title,
            ShortDescription = ShortDescription,
            Description = Description,
            IsHidden = IsHidden,
            IsNew = IsNew
        };
    }

    public FeatureEditViewModel(Feature model, FeatureOptionViewModel[] features, RepositoryOptionViewModel[] repositories)
    {
        Id = model.Id;
        ParentId = model.FeatureId;
        RepositoryId = model.RepositoryId;

        Name = model.Name;
        Title = model.Title;
        ShortDescription = model.ShortDescription;
        Description = model.Description;

        IsHidden = model.IsHidden;
        IsNew = model.IsNew;

        Features = features;
        Repositories = repositories;
    }

    public int? Id { get; init; }
    public int? ParentId { get; init; }
    public RepositoryId RepositoryId { get; init; }

    public string Name { get; init; }
    public string Title { get; init; }
    public string ShortDescription { get; init; }
    public string Description { get; init; }

    public bool IsHidden { get; init; }
    public bool IsNew { get; init; }

    public FeatureOptionViewModel[] Features { get; init; } = [];
    public RepositoryOptionViewModel[] Repositories { get; init; } = [];
}
