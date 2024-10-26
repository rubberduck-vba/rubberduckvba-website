using System.Threading.Tasks.Dataflow;

namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;

public class DataflowJoinBlock<T1,T2> : DataflowBlockBase<JoinBlock<T1, T2>, SyncContext>, IDisposable
{
    private readonly ICollection<IDisposable> _links = [];

    public DataflowJoinBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger, string name) 
        : base(parent, tokenSource, logger)
    {
        Name = name;
    }

    public override string Name { get; }

    public JoinBlock<T1, T2> Block { get; private set; } = default!;

    public JoinBlock<T1, T2> CreateBlock(ISourceBlock<T1> source1, ISourceBlock<T2> source2)
    {
        var options = new GroupingDataflowBlockOptions
        {
            CancellationToken = Token,
            Greedy = true,
        };
        return CreateBlock(options, source1, source2);
    }

    public JoinBlock<T1, T2> CreateBlock<TContext>(ISectionBlock<TContext> source1, ISectionBlock<TContext> source2)
        where TContext : IPipelineContext
    {
        var options = new GroupingDataflowBlockOptions
        {
            CancellationToken = Token,
            Greedy = true,
        };
        return CreateBlock(options, source1, source2);
    }

    public JoinBlock<T1, T2> CreateBlock(GroupingDataflowBlockOptions options, ISourceBlock<T1> source1, ISourceBlock<T2> source2)
    {
        Block = new JoinBlock<T1, T2>(options);
        LinkSources(source1, source2);

        return Block;
    }

    public JoinBlock<T1, T2> CreateBlock<TContext>(GroupingDataflowBlockOptions options, ISectionBlock<TContext> source1, ISectionBlock<TContext> source2)
        where TContext : IPipelineContext
    {
        Block = new JoinBlock<T1, T2>(options);

        var sourceBlock1 = source1.TryGetBlock() as ISourceBlock<T1>;
        var sourceBlock2 = source2.TryGetBlock() as ISourceBlock<T2>;
        if (sourceBlock1 != null && sourceBlock2 != null)
        {
            LinkSources(sourceBlock1, sourceBlock2);
        }

        return Block;
    }

    private void LinkSources(ISourceBlock<T1> source1, ISourceBlock<T2> source2)
    {
        _links.Add(source1.LinkTo(Block.Target1, new DataflowLinkOptions { PropagateCompletion = true }));
        _links.Add(source2.LinkTo(Block.Target2, new DataflowLinkOptions { PropagateCompletion = true }));
        Logger.LogTrace(Context.Parameters, $"{Name} | 🔗 Linked to {source1.GetType().Name} ({typeof(T1).Name}) and {source2.GetType().Name} ({typeof(T2).Name}) source blocks | Propagation: ON");
    }

    public void Dispose()
    {
        foreach (var link in _links)
        {
            link.Dispose();
        }
        Logger.LogTrace(Context.Parameters, $"{Name} | ✔️ Disposed {_links.Count} links");
        _links.Clear();
    }

    public override IDataflowBlock? TryGetBlock() => Block;
}

public class DataflowJoinBlock<T1, T2, T3> : DataflowBlockBase<JoinBlock<T1, T2, T3>, SyncContext>, IDisposable
{
    private readonly ICollection<IDisposable> _links = [];

    public DataflowJoinBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger, string name)
        : base(parent, tokenSource, logger)
    {
        Name = name;
    }

    public override string Name { get; }

    public JoinBlock<T1, T2, T3> Block { get; private set; } = default!;

    public JoinBlock<T1, T2, T3> CreateBlock(ISourceBlock<T1> source1, ISourceBlock<T2> source2, ISourceBlock<T3> source3)
    {
        var options = new GroupingDataflowBlockOptions
        {
            CancellationToken = Token,
            Greedy = true,
        };
        return CreateBlock(options, source1, source2, source3);
    }

    public JoinBlock<T1, T2, T3> CreateBlock<TContext>(ISectionBlock<TContext> source1, ISectionBlock<TContext> source2, ISectionBlock<TContext> source3)
        where TContext : IPipelineContext
    {
        var options = new GroupingDataflowBlockOptions
        {
            CancellationToken = Token,
            Greedy = true,
        };
        return CreateBlock(options, source1, source2, source3);
    }

    public JoinBlock<T1, T2, T3> CreateBlock(GroupingDataflowBlockOptions options, ISourceBlock<T1> source1, ISourceBlock<T2> source2, ISourceBlock<T3> source3)
    {
        Block = new JoinBlock<T1, T2, T3>(options);
        LinkSources(source1, source2, source3);

        return Block;
    }

    public JoinBlock<T1, T2, T3> CreateBlock<TContext>(GroupingDataflowBlockOptions options, ISectionBlock<TContext> source1, ISectionBlock<TContext> source2, ISectionBlock<TContext> source3)
        where TContext : IPipelineContext
    {
        Block = new JoinBlock<T1, T2, T3>(options);

        var sourceBlock1 = source1.TryGetBlock() as ISourceBlock<T1>;
        var sourceBlock2 = source2.TryGetBlock() as ISourceBlock<T2>;
        var sourceBlock3 = source3.TryGetBlock() as ISourceBlock<T3>;
        if (sourceBlock1 != null && sourceBlock2 != null && sourceBlock3 != null)
        {
            LinkSources(sourceBlock1, sourceBlock2, sourceBlock3);
        }

        return Block;
    }

    private void LinkSources(ISourceBlock<T1> source1, ISourceBlock<T2> source2, ISourceBlock<T3> source3)
    {
        _links.Add(source1.LinkTo(Block.Target1, new DataflowLinkOptions { PropagateCompletion = true }));
        _links.Add(source2.LinkTo(Block.Target2, new DataflowLinkOptions { PropagateCompletion = true }));
        _links.Add(source3.LinkTo(Block.Target3, new DataflowLinkOptions { PropagateCompletion = true }));
        Logger.LogTrace(Context.Parameters, $"{Name} | 🔗 Linked to {source1.GetType().Name} ({typeof(T1).Name}), {source2.GetType().Name} ({typeof(T2).Name}), and {source3.GetType().Name} ({typeof(T3).Name}) source blocks | Propagation: ON");
    }

    public void Dispose()
    {
        foreach (var link in _links)
        {
            link.Dispose();
        }
        Logger.LogTrace(Context.Parameters, $"{Name} | ✔️ Disposed {_links.Count} links");
        _links.Clear();
    }

    public override IDataflowBlock? TryGetBlock() => Block;
}