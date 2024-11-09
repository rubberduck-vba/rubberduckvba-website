using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Model;
using System.Xml.Linq;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class DownloadXmlTagAssetBlock : TransformBlockBase<TagAsset, (TagAsset, XDocument), SyncContext>
{
    public DownloadXmlTagAssetBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }

    public override async Task<(TagAsset, XDocument)> TransformAsync(TagAsset input)
    {
        if (input?.DownloadUrl is null)
        {
            Logger.LogWarning(Context.Parameters, "Download url for asset ID {assetId} is unexpectedly null.", input?.Id ?? 0);
            return (null!, null!);
        }
        if (Uri.TryCreate(input.DownloadUrl, UriKind.Absolute, out var uri) && uri.Host != "github.com")
        {
            throw new UriFormatException($"Unexpected host in download URL '{uri}' from asset ID {input.Id}");
        }

        using (var client = new HttpClient())
        using (var response = await client.GetAsync(uri))
        {
            if (response.IsSuccessStatusCode)
            {
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    Logger.LogInformation(Context.Parameters, $"Loading XDocument from asset {input.DownloadUrl}...");
                    var document = XDocument.Load(stream, LoadOptions.None);
                    return (input, document);
                }
            }
            else
            {
                Logger.LogWarning(Context.Parameters, $"HTTP GET ({uri}) failed with status code {(int)response.StatusCode}: {response.ReasonPhrase}");
                throw new InvalidOperationException($"Failed to retrieve asset from url '{uri}'.");
            }
        }
    }
}
