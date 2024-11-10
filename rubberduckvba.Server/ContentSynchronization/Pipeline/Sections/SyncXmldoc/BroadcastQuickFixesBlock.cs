using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Model;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class BroadcastQuickFixesBlock : BroadcastBlockBase<IEnumerable<QuickFix>, SyncContext>
{
    public BroadcastQuickFixesBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }
}

public class BroadcastAnnotationsBlock : BroadcastBlockBase<IEnumerable<Annotation>, SyncContext>
{
    public BroadcastAnnotationsBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }
}

public class BroadcastInspectionsBlock : BroadcastBlockBase<IEnumerable<Inspection>, SyncContext>
{
    public BroadcastInspectionsBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }
}
