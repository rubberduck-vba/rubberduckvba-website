using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using System.Threading.Tasks.Dataflow;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;

public abstract class BroadcastBlockBase<TInput, TContext> : ExecutionDataflowBlockBase<BroadcastBlock<TInput>, TInput, TContext>
    where TContext : IPipelineContext
{
    protected BroadcastBlockBase(PipelineSection<TContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }

    public override BroadcastBlock<TInput> CreateBlock(ExecutionDataflowBlockOptions options)
    {
        return new BroadcastBlock<TInput>(InternalBroadcast, options);
    }

    protected TInput InternalBroadcast(TInput input)
    {
        return GuardInternalFunc(Broadcast, input);
    }

    public virtual TInput Broadcast(TInput input)
    {
        return input;
    }
}
