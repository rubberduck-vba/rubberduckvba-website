using rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.com.Server.Services;

namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline.Sections.SyncTags;

public class LoadDbFeatureItemsBlock : ActionBlockBase<SyncRequestParameters, SyncContext>
{
    private IRubberduckDbService _db;

    public LoadDbFeatureItemsBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger, IRubberduckDbService db)
        : base(parent, tokenSource, logger)
    {
        _db = db;
    }

    protected override async Task ActionAsync(SyncRequestParameters input)
    {
        var items = await _db.GetXmlDocFeaturesAsync(input.RepositoryId);
        Context.LoadFeatureItems(items);
    }
}