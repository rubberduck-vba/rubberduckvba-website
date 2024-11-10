using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.ContentSynchronization.XmlDoc.Abstract;
using rubberduckvba.Server.Model;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class MergeInspectionsBlock : TransformBlockBase<IEnumerable<Inspection>, IEnumerable<Inspection>, SyncContext>
{
    private readonly IXmlDocMerge _service;

    public MergeInspectionsBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger, IXmlDocMerge service)
        : base(parent, tokenSource, logger)
    {
        _service = service;
    }

    public override IEnumerable<Inspection> Transform(IEnumerable<Inspection> input)
    {
        try
        {
            var main = Context.StagingContext.Inspections
                .Where(inspection => Context.RubberduckDbMain.Assets.Any(asset => asset.Id == inspection.TagAssetId))
                .ToList();

            var next = Context.StagingContext.Inspections
                .Where(inspection => Context.RubberduckDbNext.Assets.Any(asset => asset.Id == inspection.TagAssetId))
                .ToList();

            Logger.LogDebug(Context.Parameters, $"Merging inspections. Main x{main.Count} | Next x{next.Count}");
            var dbItems = Context.Inspections.ToDictionary(e => e.Name);
            var merged = _service.Merge(dbItems, main, next);
            Logger.LogDebug(Context.Parameters, $"Merged x{merged.Count()}");

            Context.StagingContext.Inspections = new(merged);

            Logger.LogDebug(Context.Parameters, $"Updated: {Context.StagingContext.Inspections.Count(e => e.Id != default)} | New: {Context.StagingContext.Inspections.Count(e => e.Id == default)}");
            return merged;
        }
        finally
        {
            Block.Complete();
        }
    }
}
