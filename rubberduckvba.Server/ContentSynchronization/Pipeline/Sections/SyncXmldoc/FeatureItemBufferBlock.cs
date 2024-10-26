using rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.com.Server.Data;

namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class FeatureItemBufferBlock : BufferBlockBase<FeatureXmlDoc, SyncContext>
{
    public FeatureItemBufferBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger) 
        : base(parent, tokenSource, logger){ }
}
