using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Services;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncTags;

public class LoadInspectionDefaultConfigBlock : ActionBlockBase<SyncRequestParameters, SyncContext>
{
    private readonly IGitHubClientService _github;

    public LoadInspectionDefaultConfigBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, IGitHubClientService github, ILogger logger)
        : base(parent, tokenSource, logger)
    {
        _github = github;
    }

    protected override async Task ActionAsync(SyncRequestParameters input)
    {
        var config = await _github.GetCodeAnalysisDefaultsConfigAsync();
        Context.LoadInspectionDefaultConfig(config);
    }
}
