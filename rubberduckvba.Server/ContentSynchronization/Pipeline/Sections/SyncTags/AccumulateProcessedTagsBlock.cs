using rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.com.Server.Data;
using rubberduckvba.com.Server.Services;

namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline.Sections.SyncTags;

public class AccumulateProcessedTagsBlock : TransformBlockBase<TagGraph, SyncContext, SyncContext>
{
    public AccumulateProcessedTagsBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }

    public override SyncContext Transform(TagGraph input)
    {
        Context.StagingContext.ProcessedTags.Add(input);
        return Context;
    }
}

public class BulkSaveStagingBlock : ActionBlockBase<SyncContext, SyncContext>
{
    private readonly IStagingServices _staging;

    public BulkSaveStagingBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, IStagingServices staging, ILogger logger)
        : base(parent, tokenSource, logger)
    {
        _staging = staging;
    }

    protected override async Task ActionAsync(SyncContext input)
    {
        await _staging.StageAsync(Context.StagingContext, Token);
    }
}
