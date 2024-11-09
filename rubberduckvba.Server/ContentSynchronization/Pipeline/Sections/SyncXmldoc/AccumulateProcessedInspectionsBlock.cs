using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Model;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class AccumulateProcessedInspectionsBlock : ActionBlockBase<Inspection, SyncContext>
{
    public AccumulateProcessedInspectionsBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }

    protected override void Action(Inspection input)
    {
        if (input is null)
        {
            return;
        }

        Context.StagingContext.Inspections.Add(input);
    }
}

public class AccumulateProcessedQuickFixesBlock : ActionBlockBase<QuickFix, SyncContext>
{
    public AccumulateProcessedQuickFixesBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }

    protected override void Action(QuickFix input)
    {
        if (input is null)
        {
            return;
        }

        Context.StagingContext.QuickFixes.Add(input);
    }
}

public class AccumulateProcessedAnnotationsBlock : ActionBlockBase<Annotation, SyncContext>
{
    public AccumulateProcessedAnnotationsBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }

    protected override void Action(Annotation input)
    {
        if (input is null)
        {
            return;
        }

        Context.StagingContext.Annotations.Add(input);
    }
}
