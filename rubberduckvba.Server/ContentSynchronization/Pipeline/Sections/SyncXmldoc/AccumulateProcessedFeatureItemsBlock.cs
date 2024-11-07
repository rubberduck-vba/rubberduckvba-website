using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Data;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class AccumulateProcessedFeatureItemsBlock : ActionBlockBase<FeatureXmlDoc, SyncContext>
{
    public AccumulateProcessedFeatureItemsBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }

    protected override void Action(FeatureXmlDoc input)
    {
        if (input != null)
        {
            //foreach (var featureItem in input.Examples.GroupBy(e => e))
            //{
            //    var exampleNumber = 1;
            //    foreach (var example in featureItem)
            //    {
            //        example.SortOrder = exampleNumber;
            //        exampleNumber++;
            //    }
            //}

            Context.StagingContext.ProcessedFeatureItems.Add(input);
        }
    }
}
