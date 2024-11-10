using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Model;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncTags;

public class StreamGitHubOtherTagsBlock : TransformManyBlockBase<Tuple<SyncContext, SyncContext>, TagGraph, SyncContext>
{
    public StreamGitHubOtherTagsBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }

    public override IEnumerable<TagGraph> Transform(Tuple<SyncContext, SyncContext> input)
    {
        return Context.GitHubOtherTags;
    }
}
