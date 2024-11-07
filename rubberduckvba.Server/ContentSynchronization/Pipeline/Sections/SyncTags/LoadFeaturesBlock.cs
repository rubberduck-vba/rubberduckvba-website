using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Services;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncTags;

public class LoadFeaturesBlock : ActionBlockBase<SyncRequestParameters, SyncContext>
{
    private readonly IRubberduckDbService _content;

    public LoadFeaturesBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, IRubberduckDbService content, ILogger logger)
        : base(parent, tokenSource, logger)
    {
        _content = content;
    }

    protected override async Task ActionAsync(SyncRequestParameters input)
    {
        var inspections = await _content.ResolveFeature(input.RepositoryId, "inspections");
        var quickfixes = await _content.ResolveFeature(input.RepositoryId, "quickfixes");
        var annotations = await _content.ResolveFeature(input.RepositoryId, "annotations");

        Context.LoadFeatures([inspections, quickfixes, annotations]);
    }
}
