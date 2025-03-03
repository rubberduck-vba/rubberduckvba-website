using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Services;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncTags;

public class LoadGitHubTagsBlock : TransformBlockBase<SyncContext, SyncContext, SyncContext>
{
    private readonly IGitHubClientService _github;

    public LoadGitHubTagsBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, IGitHubClientService github, ILogger logger)
        : base(parent, tokenSource, logger)
    {
        _github = github;
    }

    public override async Task<SyncContext> TransformAsync(SyncContext input)
    {
        var githubTags = await _github.GetAllTagsAsync(Context.RubberduckDbMain.Name);
        var (gitHubMain, gitHubNext, gitHubOthers) = githubTags.GetLatestTags();

        Context.LoadGitHubTags(gitHubMain, gitHubNext, gitHubOthers);
        return Context;
    }
}
