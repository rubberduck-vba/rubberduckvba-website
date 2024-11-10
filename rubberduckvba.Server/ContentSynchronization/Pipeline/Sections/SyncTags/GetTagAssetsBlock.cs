using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Model;
using rubberduckvba.Server.Services;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncTags;

public class GetTagAssetsBlock : TransformBlockBase<TagGraph, TagGraph, SyncContext>
{
    private readonly IGitHubClientService _github;

    public GetTagAssetsBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, IGitHubClientService github, ILogger logger)
        : base(parent, tokenSource, logger)
    {
        _github = github;
    }

    public override async Task<TagGraph> TransformAsync(TagGraph input)
    {
        var tag = await _github.GetTagAsync(null!, input.Name);
        var id = default(int);

        var assetIdByName = new Dictionary<string, int>();
        if (tag.Name == Context.RubberduckDbMain.Name)
        {
            assetIdByName = Context.RubberduckDbMain.Assets.ToDictionary(asset => asset.Name, asset => asset.Id);
            id = Context.RubberduckDbMain.Id;
        }
        else if (tag.Name == Context.RubberduckDbNext.Name)
        {
            assetIdByName = Context.RubberduckDbNext.Assets.ToDictionary(asset => asset.Name, asset => asset.Id);
            id = Context.RubberduckDbNext.Id;
        }

        var localAssets = new List<TagAsset>();
        foreach (var asset in tag.Assets)
        {
            var localAsset = asset with { TagId = input.Id };
            if (assetIdByName.TryGetValue(asset.Name, out var assetId))
            {
                localAsset = localAsset with { Id = assetId };
            }
            localAssets.Add(asset);
        }

        return input with { Id = id, Assets = localAssets };
    }
}
