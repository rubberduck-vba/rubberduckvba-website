using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.ContentSynchronization.XmlDoc;
using rubberduckvba.Server.Model;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class ParseQuickFixXElementInfoBlock : TransformBlockBase<(TagAsset asset, XElementInfo info), QuickFix, SyncContext>
{
    private readonly XmlDocQuickFixParser _parser;

    public ParseQuickFixXElementInfoBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger, XmlDocQuickFixParser parser)
        : base(parent, tokenSource, logger)
    {
        _parser = parser;
    }

    public override QuickFix Transform((TagAsset asset, XElementInfo info) input)
    {
        if (input.asset is null)
        {
            return null!;
        }

        var feature = Context.Features["QuickFixes"];
        var isPreRelease = Context.RubberduckDbNext.Assets.Any(asset => asset.Id == input.asset.Id);

        var result = _parser.Parse(input.info.Name, input.asset.Id, feature.Id, input.info.Element, isPreRelease) with { TagAssetId = input.asset.Id };

        return result ?? throw new InvalidOperationException("Failed to parse quickfix xmldoc");
    }
}
