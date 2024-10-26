using rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.com.Server.Services;

namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class SyncMergeBlock : ActionBlockBase<SyncContext, SyncContext>
{
    private readonly IStagingServices _staging;

    public SyncMergeBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, IStagingServices staging, ILogger logger)
        : base(parent, tokenSource, logger)
    {
        _staging = staging;
    }

    protected override async Task ActionAsync(SyncContext input)
    {
        //var results = await _staging.SyncMergeAsync(input.Parameters.RequestId);
        //foreach (var result in results)
        //{
        //    Context.SyncMergeResults.Add(result);
        //}
    }
}
