using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using System.Threading.Tasks.Dataflow;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;

public abstract class PipelineBase<TContext, TResult> : IDisposable, IPipeline<TContext, TResult>
    where TContext : IPipelineContext
{
    private readonly ICollection<IDisposable> _links = new List<IDisposable>();

    protected PipelineBase(TContext context, CancellationTokenSource tokenSource, ILogger logger)
    {
        Logger = logger;
        Context = context;
        TokenSource = tokenSource;
        Token = TokenSource.Token;

        Result = Parent?.Result ?? new PipelineResult<TResult>();
    }

    protected PipelineBase(IPipeline<TContext, TResult> parent, CancellationTokenSource tokenSource, ILogger logger)
        : this(parent.Context, tokenSource, logger)
    {
        Parent = parent;
    }

    protected ILogger Logger { get; }
    protected CancellationTokenSource TokenSource { get; }
    protected CancellationToken Token { get; }

    public TContext Context { get; }

    public IPipelineResult<TResult> Result { get; }
    public IEnumerable<Exception> Exceptions => _sections.SelectMany(section => section.Result.Exceptions).ToList();

    public virtual void Start<TInput>(ITargetBlock<TInput> entryBlock, TInput input)
    {
        Logger.LogInformation(Context.Parameters, $"{GetType().Name} | 🚀 Starting pipeline...");
        entryBlock.Post(input);
        entryBlock.Complete();
    }

    protected IPipeline<TContext, TResult>? Parent { get; }

    private readonly IList<PipelineSection<TContext>> _sections = new List<PipelineSection<TContext>>();
    protected IEnumerable<PipelineSection<TContext>> Sections => _sections;

    protected void AddSections(SyncRequestParameters parameters, params PipelineSection<TContext>[] sections)
    {
        foreach (var section in sections)
        {
            section.CreateBlocks();
            _sections.Add(section);
        }
    }

    protected void LinkSections<TTarget>(ISourceBlock<TTarget> exitBlock, ITargetBlock<TTarget> entryBlock)
    {
        _links.Add(exitBlock.LinkTo(entryBlock, new DataflowLinkOptions { PropagateCompletion = true }));
    }

    protected void LinkSections(Task exitTask, ITargetBlock<TContext> successEntryBlock, ITargetBlock<TContext>? faultedEntryBlock = null)
    {
        LinkSections(exitTask, Context, successEntryBlock, faultedEntryBlock);
    }

    protected void LinkSections<TTarget>(Task exitTask, TTarget input, ITargetBlock<TTarget> entryBlock, ITargetBlock<TTarget>? faultedEntryBlock = null)
    {
        Task.WhenAll(exitTask).ContinueWith(t =>
        {
            try
            {
                entryBlock.Post(input);
            }
            finally
            {
                entryBlock.Complete();
            }
        }, Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);

        if (faultedEntryBlock != null)
        {
            Task.WhenAll(exitTask).ContinueWith(t =>
            {
                try
                {
                    faultedEntryBlock.Post(input);
                }
                finally
                {
                    faultedEntryBlock.Complete();
                }
            }, Token, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
        }
    }

    public void DisposeAfter(Task completion)
    {
        completion.ContinueWith(t => Dispose());
    }

    protected bool _isDisposed;
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                foreach (var link in _links)
                {
                    link.Dispose();
                }
                _links.Clear();

                foreach (var section in Sections)
                {
                    section.Dispose();
                }
                _sections.Clear();

                _isDisposed = true;
            }
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}