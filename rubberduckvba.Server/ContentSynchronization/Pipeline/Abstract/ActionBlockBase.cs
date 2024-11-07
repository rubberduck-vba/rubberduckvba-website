using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using System.Threading.Tasks.Dataflow;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;

public abstract class ActionBlockBase<TInput, TContext> : ExecutionDataflowBlockBase<ActionBlock<TInput>, TInput, TContext>
    where TContext : IPipelineContext
{
    protected ActionBlockBase(PipelineSection<TContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }

    public sealed override ActionBlock<TInput> CreateBlock(ExecutionDataflowBlockOptions options)
    {
        return new ActionBlock<TInput>(InternalAction, options);
    }

    protected void InternalAction(TInput input)
    {
        GuardInternalAction(Action, input);
    }

    protected virtual void Action(TInput input)
    {
        ActionAsync(input).Wait();
    }

    protected virtual async Task ActionAsync(TInput input)
    {
        await Task.Run(() => Action(input));
    }
}
