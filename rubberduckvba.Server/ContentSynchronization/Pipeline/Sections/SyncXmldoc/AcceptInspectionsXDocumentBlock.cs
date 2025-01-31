using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Model;
using System.Xml.Linq;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class AcceptInspectionsXDocumentBlock : TransformBlockBase<(TagAsset, XDocument), SyncContext, SyncContext>
{
    public AcceptInspectionsXDocumentBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }

    public override SyncContext Transform((TagAsset, XDocument) input)
    {
        if (input.Item1.Name.Equals("Rubberduck.CodeAnalysis.xml", StringComparison.InvariantCultureIgnoreCase))
        {
            Context.LoadCodeAnalysisXDocument(input);

            if (Context.XDocuments.Any(e => e.Item1.TagId == Context.RubberduckDbMain.Id)
                && Context.XDocuments.Any(e => e.Item1.TagId == Context.RubberduckDbNext.Id))
            {
                Block.Complete();
            }
        }

        return Context;
    }
}