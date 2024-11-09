using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.ContentSynchronization.XmlDoc.Abstract;
using rubberduckvba.Server.Model;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class MergeAnnotationsBlock : TransformBlockBase<IEnumerable<Annotation>, IEnumerable<Annotation>, SyncContext>
{
    private readonly IXmlDocMerge _service;

    public MergeAnnotationsBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger, IXmlDocMerge service)
        : base(parent, tokenSource, logger)
    {
        _service = service;
    }

    public override IEnumerable<Annotation> Transform(IEnumerable<Annotation> input)
    {
        try
        {

            var main = input
                .Where(e => Context.RubberduckDbMain.Assets.Any(asset => asset.Id == e.TagAssetId))
                .ToList();

            var next = input
                .Where(e => Context.RubberduckDbNext.Assets.Any(asset => asset.Id == e.TagAssetId))
                .ToList();

            Logger.LogDebug(Context.Parameters, $"Merging annotations. Main x{main.Count} | Next x{next.Count}");
            var dbItems = Context.Annotations.ToDictionary(e => e.Name);
            var merged = _service.Merge(dbItems, main, next);
            Logger.LogDebug(Context.Parameters, $"Merged x{merged.Count()}");

            Context.StagingContext.Annotations = new(merged);

            Logger.LogDebug(Context.Parameters, $"Updated: {Context.StagingContext.Annotations.Count()} | New: {Context.StagingContext.Annotations.Count()}");
            return merged;
        }
        finally
        {
            Block.Complete();
        }
    }
}
