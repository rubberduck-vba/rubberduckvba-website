using rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.com.Server.Data;
using rubberduckvba.com.Server.Services;

namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline.Sections.SyncTags;

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
