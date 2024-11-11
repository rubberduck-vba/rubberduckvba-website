using rubberduckvba.Server.Model;

namespace rubberduckvba.Server.Api.Features;

public class FeatureViewModel
{
    public FeatureViewModel(Feature model)
    {
        Id = model.Id;
        DateInserted = model.DateTimeInserted;
        DateUpdated = model.DateTimeUpdated;

        Name = model.Name;
        Title = model.Title;
        ShortDescription = model.ShortDescription;
        Description = model.Description;
        IsNew = model.IsNew;
        IsHidden = model.IsHidden;
        HasImage = model.HasImage;

        if (model is FeatureGraph graph)
        {
            Features = graph.Features.Select(e => new FeatureViewModel(e) { FeatureId = e.ParentId, FeatureName = graph.Name, FeatureTitle = graph.Title }).ToArray();
        }
    }

    public int Id { get; init; }
    public DateTime DateInserted { get; init; }
    public DateTime? DateUpdated { get; init; }

    public int? FeatureId { get; init; }
    public string? FeatureName { get; init; }
    public string? FeatureTitle { get; init; }

    public string Name { get; init; }
    public string Title { get; init; }
    public string ShortDescription { get; init; }
    public string Description { get; init; }
    public bool IsNew { get; init; }
    public bool IsHidden { get; init; }
    public bool HasImage { get; init; }

    public FeatureViewModel[] Features { get; init; } = [];
}

public class InspectionsFeatureViewModel : FeatureViewModel
{
    public InspectionsFeatureViewModel(FeatureGraph model)
        : base(model)
    {
        Inspections = model.Inspections.ToArray();
    }

    public Inspection[] Inspections { get; init; } = [];
}

public class QuickFixesFeatureViewModel : FeatureViewModel
{
    public QuickFixesFeatureViewModel(FeatureGraph model)
        : base(model)
    {
        QuickFixes = model.QuickFixes.ToArray();
    }

    public QuickFix[] QuickFixes { get; init; } = [];
}

public class AnnotationsFeatureViewModel : FeatureViewModel
{
    public AnnotationsFeatureViewModel(FeatureGraph model)
        : base(model)
    {
        Annotations = model.Annotations.ToArray();
    }

    public Annotation[] Annotations { get; init; } = [];
}
