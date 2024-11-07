using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.ContentSynchronization.XmlDoc.Abstract;
using rubberduckvba.Server.Model;
using System.Collections.Immutable;

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

            var main = input
                .Where(e => Context.RubberduckDbMain.Assets.Any(asset => asset.Id == e.TagAssetId))
                .ToList();

            var next = input
                .Where(e => Context.RubberduckDbNext.Assets.Any(asset => asset.Id == e.TagAssetId))
                .ToList();

            Logger.LogDebug(Context.Parameters, $"Merging inspections. Main x{main.Count} | Next x{next.Count}");
            var dbItems = Context.Inspections.ToDictionary(e => e.Name);
            var merged = _service.Merge(dbItems, main, next);
            Logger.LogDebug(Context.Parameters, $"Merged x{merged.Count()}");

            Context.StagingContext.UpdatedInspections = merged.Where(e => e.Id != default && e.DateTimeUpdated.HasValue).ToImmutableArray();
            Context.StagingContext.NewInspections = merged.Where(e => e.Id == default).ToImmutableArray();

            Logger.LogDebug(Context.Parameters, $"Updated: {Context.StagingContext.UpdatedInspections.Count()} | New: {Context.StagingContext.NewInspections.Count()}");
            return merged;
        }
        finally
        {
            Block.Complete();
        }
    }
}
