using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Model;
using System.Xml.Linq;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class GetAnnotationNodesBlock : GetXElementInfoBlock
{
    public GetAnnotationNodesBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }

    public override IEnumerable<(TagAsset, XElementInfo)> Transform((TagAsset asset, XDocument document) input)
    {
        if (input.asset is null || input.document is null)
        {
            return [];
        }

        var result = (from node in input.document.Descendants("member").AsParallel()
                      let fullName = GetNameOrDefault(node, "Annotation")
                      where !string.IsNullOrWhiteSpace(fullName)
                      let typeName = fullName.Substring(fullName.LastIndexOf(".", StringComparison.Ordinal) + 1)
                      select (input.asset, new XElementInfo(typeName, node)))
                    .ToList();

        Logger.LogInformation(Context.Parameters, $"{Name} | Extracted {result.Count} annotation nodes from tag asset {input.asset.Name}");
        return result;
    }
}
