using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Model;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncTags;

public class StreamGitHubTagsBlock : TransformManyBlockBase<Tuple<SyncContext, SyncContext>, TagGraph, SyncContext>
{
    public StreamGitHubTagsBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }

    public override IEnumerable<TagGraph> Transform(Tuple<SyncContext, SyncContext> input)
    {
        return [Context.GitHubMain, Context.GitHubNext];
    }
}
