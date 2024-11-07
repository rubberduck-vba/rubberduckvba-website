using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Model;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class GetTagAssetBlock : TransformBlockBase<TagGraph, TagAsset, SyncContext>
{
    private readonly string _name;

    public GetTagAssetBlock(string name, PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
        _name = name;
    }

    public override TagAsset Transform(TagGraph input)
    {
        var asset = input.Assets.SingleOrDefault(e => e.Name == _name);
        if (asset != null)
        {
            Logger.LogInformation($"Found {asset.Name} asset for tag {input.Name}");
        }
        return asset;
    }
}
