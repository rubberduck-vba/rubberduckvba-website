using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.ContentSynchronization.XmlDoc;
using rubberduckvba.Server.Model;
using rubberduckvba.Server.Services;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class ParseInspectionXElementInfoBlock : TransformBlockBase<(TagAsset asset, XElementInfo info), Inspection, SyncContext>
{
    private readonly IMarkdownFormattingService _markdownService;

    public ParseInspectionXElementInfoBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger,
        IMarkdownFormattingService markdownService)
        : base(parent, tokenSource, logger)
    {
        _markdownService = markdownService;
    }

    public override async Task<Inspection> TransformAsync((TagAsset asset, XElementInfo info) input)
    {
        if (input.asset is null)
        {
            return default!;
        }

        var feature = Context.Features["Inspections"];
        var quickfixes = Context.Features["QuickFixes"].QuickFixes; // FIXME not loaded in initial run
        var config = Context.InspectionDefaultConfig.TryGetValue(input.info.Name, out var value) ? value : null;
        var isPreRelease = Context.RubberduckDbNext.Assets.Any(asset => asset.Id == input.asset.Id);
        var name = input.info.Element.Attribute("name")!.Value;

        if (Context.TryGetTagById(input.asset.TagId, out var tag))
        {
            var parser = new XmlDocInspection(_markdownService);
            var result = await parser.ParseAsync(input.asset.Id, tag.Name, feature.Id, quickfixes, name, input.info.Element, config, isPreRelease);

            return result with { TagAssetId = input.asset.Id };
        }

        throw new InvalidOperationException($"[Tag.Id]:{input.asset.TagId} was not loaded or does not exist.");
    }
}
