using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Data;
using rubberduckvba.Server.Services;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

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
