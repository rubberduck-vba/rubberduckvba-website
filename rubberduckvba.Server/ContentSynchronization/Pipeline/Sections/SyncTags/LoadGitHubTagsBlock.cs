using rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.com.Server.Services;

namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline.Sections.SyncTags;

public class LoadGitHubTagsBlock : TransformBlockBase<SyncRequestParameters, SyncContext, SyncContext>
{
    private readonly IGitHubClientService _github;

    public LoadGitHubTagsBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, IGitHubClientService github, ILogger logger)
        : base(parent, tokenSource, logger)
    {
        _github = github;
    }

    public override async Task<SyncContext> TransformAsync(SyncRequestParameters input)
    {
        var githubTags = await _github.GetAllTagsAsync(); // does not get the assets
        var (gitHubMain, gitHubNext, gitHubOthers) = githubTags.GetLatestTags();
        
        Context.LoadGitHubTags(gitHubMain, gitHubNext, gitHubOthers);
        return Context;
    }
}
