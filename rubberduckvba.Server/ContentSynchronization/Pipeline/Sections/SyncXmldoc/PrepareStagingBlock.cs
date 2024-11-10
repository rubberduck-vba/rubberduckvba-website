using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Model;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class PrepareStagingBlock : TransformBlockBase<Tuple<IEnumerable<Annotation>, IEnumerable<QuickFix>, IEnumerable<Inspection>>, StagingContext, SyncContext>
{
    public PrepareStagingBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }

    public override StagingContext Transform(Tuple<IEnumerable<Annotation>, IEnumerable<QuickFix>, IEnumerable<Inspection>> input)
    {
        var parameters = Context.StagingContext.Parameters;
        var (annotations, quickfixes, inspections) = (input.Item1, input.Item2, input.Item3);

        return new StagingContext(parameters)
        {
            Inspections = new(inspections),
            QuickFixes = new(quickfixes),
            Annotations = new(annotations)
        };
    }
}