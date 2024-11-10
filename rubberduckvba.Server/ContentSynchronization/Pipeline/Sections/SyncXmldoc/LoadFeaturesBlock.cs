using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Services;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class LoadFeaturesBlock : TransformBlockBase<SyncRequestParameters, SyncContext, SyncContext>
{
    private readonly IRubberduckDbService _content;

    public LoadFeaturesBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, IRubberduckDbService content, ILogger logger)
        : base(parent, tokenSource, logger)
    {
        _content = content;
    }

    public override async Task<SyncContext> TransformAsync(SyncRequestParameters input)
    {
        var inspections = await _content.ResolveFeature(input.RepositoryId, "inspections");
        var quickfixes = await _content.ResolveFeature(input.RepositoryId, "quickfixes");
        var annotations = await _content.ResolveFeature(input.RepositoryId, "annotations");

        Context.LoadFeatures([inspections, quickfixes, annotations]);
        return Context;
    }
}
