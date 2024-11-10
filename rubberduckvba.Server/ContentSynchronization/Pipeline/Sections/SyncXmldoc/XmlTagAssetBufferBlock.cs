using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Model;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class XmlTagAssetBufferBlock : BufferBlockBase<TagAsset, SyncContext>
{
    public XmlTagAssetBufferBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger) { }
}

public class AnnotationBufferBlock : BufferBlockBase<IEnumerable<Annotation>, SyncContext>
{
    public AnnotationBufferBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger) { }
}

public class QuickFixBufferBlock : BufferBlockBase<IEnumerable<QuickFix>, SyncContext>
{
    public QuickFixBufferBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger) { }
}

public class InspectionBufferBlock : BufferBlockBase<IEnumerable<Inspection>, SyncContext>
{
    public InspectionBufferBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger) { }
}
