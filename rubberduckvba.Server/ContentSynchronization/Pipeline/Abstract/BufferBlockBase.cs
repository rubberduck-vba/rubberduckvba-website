using System.Threading.Tasks.Dataflow;

namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;

public abstract class BufferBlockBase<TInput, TContext> : ExecutionDataflowBlockBase<BufferBlock<TInput>, TInput, TContext>
    where TContext : IPipelineContext
{
    protected BufferBlockBase(PipelineSection<TContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }

    public sealed override BufferBlock<TInput> CreateBlock(ExecutionDataflowBlockOptions options)
    {
        return new BufferBlock<TInput>(options);
    }
}
