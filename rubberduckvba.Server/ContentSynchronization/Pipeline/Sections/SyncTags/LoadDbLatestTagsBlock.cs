using rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.com.Server.Data;

namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline.Sections.SyncTags;

public class LoadDbLatestTagsBlock : TransformBlockBase<Tuple<TagGraph, TagGraph>, SyncContext, SyncContext>
{
    public LoadDbLatestTagsBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger) 
        : base(parent, tokenSource, logger)
    {
    }

    public override SyncContext Transform(Tuple<TagGraph, TagGraph> input)
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

        return Context;
    }
}
