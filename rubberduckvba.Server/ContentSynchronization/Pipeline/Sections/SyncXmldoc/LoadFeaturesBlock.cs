using rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.com.Server.Services;

namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

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
