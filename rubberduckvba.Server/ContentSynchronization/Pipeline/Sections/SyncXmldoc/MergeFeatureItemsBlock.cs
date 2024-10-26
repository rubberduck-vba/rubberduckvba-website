using rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.com.Server.ContentSynchronization.XmlDoc.Abstract;
using rubberduckvba.com.Server.Data;
using System.Collections.Immutable;

namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class MergeFeatureItemsBlock : TransformBlockBase<IEnumerable<FeatureXmlDoc>, IEnumerable<FeatureXmlDoc>, SyncContext>
{
    private readonly IXmlDocMerge _service;

    public MergeFeatureItemsBlock(IXmlDocMerge merge, PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
        _service = merge;
    }

    public override IEnumerable<FeatureXmlDoc> Transform(IEnumerable<FeatureXmlDoc> input)
    {
        try
        {
            var main = input
                .Where(e => Context.RubberduckDbMain.Assets.Any(asset => asset.TagId == e.TagId && asset.Id == e.TagAssetId))
                .ToList();

            var next = input
                .Where(e => Context.RubberduckDbNext.Assets.Any(asset => asset.TagId == e.TagId && asset.Id == e.TagAssetId))
                .ToList();

            Logger.LogDebug(Context.Parameters, $"Merging feature items. Main x{main.Count} | Next x{next.Count}");
            var dbItems = Context.FeatureItems.ToDictionary(e => e.Name);
            var merged = _service.Merge(dbItems, main, next);
            Logger.LogDebug(Context.Parameters, $"Merged x{merged.Count()}");

            Context.StagingContext.UpdatedFeatureItems = merged.Where(e => e.Id != default && e.DateTimeUpdated.HasValue).ToImmutableArray();
            Context.StagingContext.NewFeatureItems = merged.Where(e => e.Id == default).ToImmutableArray();

            Logger.LogDebug(Context.Parameters, $"Updated: {Context.StagingContext.UpdatedFeatureItems.Count} | New: {Context.StagingContext.NewFeatureItems.Count}");
            return merged;
        }
        finally
        {
            Block.Complete();
        }
    }
}
