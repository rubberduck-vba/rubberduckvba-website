using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.ContentSynchronization.XmlDoc;
using rubberduckvba.Server.Model;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class ParseInspectionXElementInfoBlock : TransformBlockBase<(TagAsset asset, XElementInfo info, IEnumerable<QuickFix> quickfixes), Inspection, SyncContext>
{
    private readonly XmlDocInspectionParser _parser;

    public ParseInspectionXElementInfoBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger,
        XmlDocInspectionParser parser)
        : base(parent, tokenSource, logger)
    {
        _parser = parser;
    }

    public override async Task<Inspection> TransformAsync((TagAsset asset, XElementInfo info, IEnumerable<QuickFix> quickfixes) input)
    {
        if (input.asset is null)
        {
            return default!;
        }

        var feature = Context.Features["Inspections"];

        var config = Context.InspectionDefaultConfig.TryGetValue(input.info.Name, out var value) ? value : null;
        var isPreRelease = Context.RubberduckDbNext.Assets.Any(asset => asset.Id == input.asset.Id);
        var name = input.info.Element.Attribute("name")!.Value;

        var result = await _parser.ParseAsync(input.asset.Id, feature.Id, input.quickfixes, name, input.info.Element, config, isPreRelease);

        return result with { TagAssetId = input.asset.Id };
    }
}
