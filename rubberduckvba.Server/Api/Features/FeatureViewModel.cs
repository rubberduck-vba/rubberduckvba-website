using rubberduckvba.com.Server.Data;
using System.Text.Json;

namespace rubberduckvba.com.Server.Api.Features;

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
            FeatureId = graph.ParentId;
            FeatureName = graph.ParentName;
            FeatureTitle = graph.ParentTitle;

            Features = graph.Features.Select(e => new FeatureViewModel(e)).ToArray();
            Items = graph.Items.Select(e => e with { FeatureId = graph.ParentId!.Value, FeatureName = graph.ParentName, FeatureTitle = graph.ParentTitle }).ToArray();
            Inspections = graph.Items.Select(e => JsonSerializer.Deserialize<XmlDocInspectionInfo>(e.Serialized)!).ToArray();
        }
    }

    public int Id { get; init; }
    public DateTime DateInserted { get; init; }
    public DateTime? DateUpdated { get; init; }

    public int? FeatureId { get; init; }
    public string FeatureName { get; init; }
    public string FeatureTitle { get; init; }

    public string Name { get; init; }
    public string Title { get; init; }
    public string ShortDescription { get; init; }
    public string Description { get; init; }
    public bool IsNew { get; init; }
    public bool IsHidden { get; init; }
    public bool HasImage { get; init; }

    public FeatureViewModel[] Features { get; init; } = [];
    public FeatureXmlDoc[] Items { get; init; } = [];
    public XmlDocInspectionInfo[] Inspections { get; init; } = [];
}

public class FeatureXmlDocViewModel
{

}