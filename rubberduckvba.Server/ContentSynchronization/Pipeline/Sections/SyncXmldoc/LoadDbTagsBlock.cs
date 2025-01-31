using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Model;
using rubberduckvba.Server.Services.rubberduckdb;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class AcquireDbTagsBlock : TransformBlockBase<SyncRequestParameters, IEnumerable<Tag>, SyncContext>
{
    private readonly TagServices _service;

    public AcquireDbTagsBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger,
        TagServices service)
        : base(parent, tokenSource, logger)
    {
        _service = service;
    }

    public override IEnumerable<Tag> Transform(SyncRequestParameters input)
    {
        return _service.GetAllTags();
    }
}

public class LoadDbTagsBlock : TransformBlockBase<Tuple<TagGraph, TagGraph, IEnumerable<Tag>>, SyncContext, SyncContext>
{
    public LoadDbTagsBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }

    public override SyncContext Transform(Tuple<TagGraph, TagGraph, IEnumerable<Tag>> input)
    {
        var tags = new TagGraph[]
        {
            input.Item1,
            input.Item2,
        };
        var main = tags.Single(e => !e.IsPreRelease);
        var next = tags.Single(e => e.IsPreRelease);

        Context.LoadRubberduckDbMain(main);
        Context.LoadRubberduckDbNext(next);
        Context.LoadDbTags(input.Item3);

        return Context;
    }
}
