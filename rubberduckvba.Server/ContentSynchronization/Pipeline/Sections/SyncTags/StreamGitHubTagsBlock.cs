using rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.com.Server.Data;

namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline.Sections.SyncTags;

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
