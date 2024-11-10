using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Model;
using System.Xml.Linq;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class StreamInspectionNodesBlock : TransformManyBlockBase<Tuple<(TagAsset, XDocument), IEnumerable<QuickFix>>, (TagAsset, XElementInfo, IEnumerable<QuickFix>), SyncContext>
{
    public StreamInspectionNodesBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }

    public override IEnumerable<(TagAsset, XElementInfo, IEnumerable<QuickFix>)> Transform(Tuple<(TagAsset, XDocument), IEnumerable<QuickFix>> input)
    {
        if (input.Item1.Item1 is null)
        {
            return [];
        }

        var result = (from node in input.Item1.Item2.Descendants("member").AsParallel()
                      let fullName = GetNameOrDefault(node, "Inspection")
                      where !string.IsNullOrWhiteSpace(fullName)
                      let typeName = fullName.Substring(fullName.LastIndexOf(".", StringComparison.Ordinal) + 1)
                      select (input.Item1.Item1, new XElementInfo(typeName, node), input.Item2))
                    .ToList();

        Logger.LogInformation(Context.Parameters, $"{Name} | Extracted {result.Count} inspection nodes from tag asset {input.Item1.Item1.Name}");
        return result;
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
