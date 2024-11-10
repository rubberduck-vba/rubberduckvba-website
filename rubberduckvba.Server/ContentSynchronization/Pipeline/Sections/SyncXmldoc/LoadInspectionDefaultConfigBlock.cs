using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Services;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class LoadInspectionDefaultConfigBlock : TransformBlockBase<SyncRequestParameters, SyncContext, SyncContext>
{
    private readonly IGitHubClientService _github;

    public LoadInspectionDefaultConfigBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, IGitHubClientService github, ILogger logger)
        : base(parent, tokenSource, logger)
    {
        _github = github;
    }

    public override async Task<SyncContext> TransformAsync(SyncRequestParameters input)
    {
        var config = await _github.GetCodeAnalysisDefaultsConfigAsync();
        Context.LoadInspectionDefaultConfig(config);

        return Context;
    }
}
