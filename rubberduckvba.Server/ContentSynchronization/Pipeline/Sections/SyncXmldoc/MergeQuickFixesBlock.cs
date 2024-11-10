using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.ContentSynchronization.XmlDoc.Abstract;
using rubberduckvba.Server.Model;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class MergeQuickFixesBlock : TransformBlockBase<IEnumerable<QuickFix>, IEnumerable<QuickFix>, SyncContext>
{
    private readonly IXmlDocMerge _service;

    public MergeQuickFixesBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger, IXmlDocMerge service)
        : base(parent, tokenSource, logger)
    {
        _service = service;
    }

    public override IEnumerable<QuickFix> Transform(IEnumerable<QuickFix> input)
    {
        try
        {
            var main = Context.StagingContext.QuickFixes
                .Where(quickfix => Context.RubberduckDbMain.Assets.Any(asset => asset.Id == quickfix.TagAssetId))
                .ToList();

            var next = Context.StagingContext.QuickFixes
                .Where(e => Context.RubberduckDbNext.Assets.Any(asset => asset.Id == e.TagAssetId))
                .ToList();

            Logger.LogDebug(Context.Parameters, $"Merging quickfixes. Main x{main.Count} | Next x{next.Count}");
            var dbItems = Context.QuickFixes.ToDictionary(e => e.Name);
            var merged = _service.Merge(dbItems, main, next);
            Logger.LogDebug(Context.Parameters, $"Merged x{merged.Count()}");

            Context.StagingContext.QuickFixes = new(merged);

            Logger.LogDebug(Context.Parameters, $"Updated: {Context.StagingContext.QuickFixes.Count(e => e.Id != default)} | New: {Context.StagingContext.QuickFixes.Count(e => e.Id == default)}");
            return merged;
        }
        finally
        {
            Block.Complete();
        }
    }
}
