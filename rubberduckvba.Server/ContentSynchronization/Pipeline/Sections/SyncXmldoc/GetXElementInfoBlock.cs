using System.Xml.Linq;
using rubberduckvba.com.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.com.Server.Data;

namespace rubberduckvba.com.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

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
