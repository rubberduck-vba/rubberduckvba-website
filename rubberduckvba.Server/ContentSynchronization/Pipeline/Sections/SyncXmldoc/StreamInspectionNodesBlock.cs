using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Model;
using System.Xml.Linq;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class StreamInspectionNodesBlock : TransformManyBlockBase<SyncContext, (TagAsset, XElementInfo, IEnumerable<QuickFix>), SyncContext>
{
    public StreamInspectionNodesBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }

    public override IEnumerable<(TagAsset, XElementInfo, IEnumerable<QuickFix>)> Transform(SyncContext context)
    {
        foreach (var xmldoc in context.XDocuments.Select(x => (asset: x.Item1, nodes: x.Item2)))
        {
            foreach (var result in
                from node in xmldoc.nodes.Descendants("member")
                let fullName = GetNameOrDefault(node, "Inspection")
                where !string.IsNullOrWhiteSpace(fullName)
                let typeName = fullName.Substring(fullName.LastIndexOf(".", StringComparison.Ordinal) + 1)
                select (xmldoc.asset, new XElementInfo(typeName, node), context.StagingContext.QuickFixes))
            {
                yield return result;
            }

        }
    }

    private static string GetNameOrDefault(XElement memberNode, string suffix)
    {
        var name = memberNode.Attribute("name")?.Value;
        if (name == null || !name.StartsWith("T:") || !name.EndsWith(suffix) || name.EndsWith($"I{suffix}"))
        {
            return default!;
        }

        return name.Substring(2);
    }
}
