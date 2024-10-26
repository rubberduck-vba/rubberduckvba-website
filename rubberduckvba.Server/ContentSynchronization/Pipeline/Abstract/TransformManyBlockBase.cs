using System.Threading.Tasks.Dataflow;

namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;

public abstract class TransformManyBlockBase<TInput, TOutput, TContext> : ExecutionDataflowBlockBase<TransformManyBlock<TInput, TOutput>, TInput, TContext>
    where TContext : IPipelineContext
{
    protected TransformManyBlockBase(PipelineSection<TContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger) 
    {
    }

    public override TransformManyBlock<TInput, TOutput> CreateBlock(ExecutionDataflowBlockOptions options) => new(TransformInternal, options);

    private IEnumerable<TOutput> TransformInternal(TInput input) => GuardInternalFunc(Transform, input);

    public abstract IEnumerable<TOutput> Transform(TInput input);
}
