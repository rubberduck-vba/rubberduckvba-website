using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using System.Threading.Tasks.Dataflow;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;

public interface ISectionBlock<TContext>
    where TContext : IPipelineContext
{
    string Name { get; }
    IDataflowBlock? TryGetBlock();
}

public abstract class DataflowBlockBase<TBlock, TContext> : ISectionBlock<TContext>
    where TBlock : class, IDataflowBlock
    where TContext : IPipelineContext
{
    protected DataflowBlockBase(PipelineSection<TContext> parent, CancellationTokenSource tokenSource, ILogger logger)
    {
        Parent = parent;

        Logger = logger;

        TokenSource = tokenSource;
        Token = TokenSource.Token;

        Name = GetType().Name;
    }

    public abstract IDataflowBlock? TryGetBlock();

    public virtual string Name { get; }
    public string BlockTypeName { get; } = typeof(TBlock).Name;

    protected ILogger Logger { get; }

    protected PipelineSection<TContext> Parent { get; }
    protected TContext Context => Parent.Context;
    protected CancellationTokenSource TokenSource { get; }
    protected CancellationToken Token { get; }
}
