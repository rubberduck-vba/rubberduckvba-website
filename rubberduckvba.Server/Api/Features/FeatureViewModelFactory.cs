using rubberduckvba.Server.Model;
using rubberduckvba.Server.Services;

namespace rubberduckvba.Server.Api.Features;

public class FeatureViewModelFactory(IMarkdownFormattingService md)
{
    public FeatureViewModel[] Create(IEnumerable<Feature> models) => models.Select(Create).ToArray();

    public FeatureViewModel Create(Feature model) => new(model);

    public T Create<T>(FeatureGraph model) where T : FeatureViewModel =>
        // TODO format markdown before returning
        (T)(model is not FeatureGraph graph ? new FeatureViewModel(model) : model.Name switch
        {
            "Inspections" => new InspectionsFeatureViewModel(graph),
            "QuickFixes" => new QuickFixesFeatureViewModel(graph),
            "Annotations" => new AnnotationsFeatureViewModel(graph),
            _ => new FeatureViewModel(model)
        });
}
