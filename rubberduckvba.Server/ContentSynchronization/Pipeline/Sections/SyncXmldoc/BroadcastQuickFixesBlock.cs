using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Model;
using System.Xml.Linq;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class BroadcastQuickFixesBlock : BroadcastBlockBase<(TagAsset, XDocument, IEnumerable<QuickFix>), SyncContext>
{
    public BroadcastQuickFixesBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger) 
        : base(parent, tokenSource, logger)
    {
    }
}
