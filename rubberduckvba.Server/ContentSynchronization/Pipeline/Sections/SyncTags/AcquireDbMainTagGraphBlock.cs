using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Model;
using rubberduckvba.Server.Services;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncTags;

public class AcquireDbMainTagGraphBlock : TransformBlockBase<SyncRequestParameters, TagGraph, SyncContext>
{
    private readonly IRubberduckDbService _content;

    public AcquireDbMainTagGraphBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, IRubberduckDbService content, ILogger logger)
        : base(parent, tokenSource, logger)
    {
        _content = content;
    }

    public override async Task<TagGraph> TransformAsync(SyncRequestParameters input)
    {
        return await _content.GetLatestTagAsync(RepositoryId.Rubberduck, includePreRelease: false);
    }
}
