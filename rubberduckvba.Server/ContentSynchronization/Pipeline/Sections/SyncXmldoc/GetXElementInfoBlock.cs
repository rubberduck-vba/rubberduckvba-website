using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Model;
using System.Xml.Linq;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public abstract class GetXElementInfoBlock : TransformManyBlockBase<(TagAsset asset, XDocument document), (TagAsset, XElementInfo), SyncContext>
{
    protected GetXElementInfoBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }

    protected static string GetNameOrDefault(XElement memberNode, string suffix)
    {
        var name = memberNode.Attribute("name")?.Value;
        if (name == null || !name.StartsWith("T:") || !name.EndsWith(suffix) || name.EndsWith($"I{suffix}"))
        {
            return default!;
        }

        return name.Substring(2);
    }
}
