using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using System.Threading.Tasks.Dataflow;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;

public abstract class TransformBlockBase<TInput, TOutput, TContext> : ExecutionDataflowBlockBase<TransformBlock<TInput, TOutput>, TInput, TContext>
    where TContext : IPipelineContext
{
    protected TransformBlockBase(PipelineSection<TContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }


    public sealed override TransformBlock<TInput, TOutput> CreateBlock(ExecutionDataflowBlockOptions options)
    {
        return new TransformBlock<TInput, TOutput>(TransformInternal, options);
    }

    protected TOutput TransformInternal(TInput input)
    {
        return GuardInternalFunc(Transform, input);
    }

    public virtual TOutput Transform(TInput input)
    {
        return TransformAsync(input).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public virtual async Task<TOutput> TransformAsync(TInput input)
    {
        return await Task.FromResult(Transform(input));
    }
}
