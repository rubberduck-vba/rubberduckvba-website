using rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.com.Server.Data;
using System.Xml.Linq;

namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class GetInspectionNodesBlock : GetXElementInfoBlock
{
    public GetInspectionNodesBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }

    public override IEnumerable<(TagAsset, XElementInfo)> Transform((TagAsset asset, XDocument document) input)
    {
        if (input.asset is null)
        {
            return [];
        }

        var result = (from node in input.document.Descendants("member").AsParallel()
                     let fullName = GetNameOrDefault(node, "Inspection")
                     where !string.IsNullOrWhiteSpace(fullName)
                     let typeName = fullName.Substring(fullName.LastIndexOf(".", StringComparison.Ordinal) + 1)
                     select (input.asset, new XElementInfo(typeName, node)))
                    .ToList();

        Logger.LogInformation(Context.Parameters, $"{Name} | Extracted {result.Count} inspection nodes from tag asset {input.asset.Name}");
        return result;
    }
}
