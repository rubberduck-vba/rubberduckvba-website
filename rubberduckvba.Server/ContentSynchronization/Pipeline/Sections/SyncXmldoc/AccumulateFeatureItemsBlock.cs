using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Data;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class AccumulateFeatureItemsBlock : ActionBlockBase<FeatureXmlDoc, SyncContext>
{
    public AccumulateFeatureItemsBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }

    protected override void Action(FeatureXmlDoc input)
    {
        if (input != null)
        {
            Context.StagingContext.ProcessedFeatureItems.Add(input);
        }
    }
}
