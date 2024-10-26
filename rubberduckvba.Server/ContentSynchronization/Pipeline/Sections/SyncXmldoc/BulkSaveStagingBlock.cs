using rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.com.Server.Data;
using rubberduckvba.com.Server.Services;

namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class BulkSaveStagingBlock : ActionBlockBase<IEnumerable<FeatureXmlDoc>, SyncContext>
{
    private readonly IStagingServices _staging;

    public BulkSaveStagingBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, IStagingServices staging, ILogger logger)
        : base(parent, tokenSource, logger)
    {
        _staging = staging;
    }

    protected override async Task ActionAsync(IEnumerable<FeatureXmlDoc> input)
    {
        await _staging.StageAsync(Context.StagingContext, Token);
    }
}
