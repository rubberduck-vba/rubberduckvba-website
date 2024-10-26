using rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.com.Server.Data;

namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline.Sections.SyncTags;

public class TagBufferBlock : BufferBlockBase<TagGraph, SyncContext>
{
    public TagBufferBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }
}
