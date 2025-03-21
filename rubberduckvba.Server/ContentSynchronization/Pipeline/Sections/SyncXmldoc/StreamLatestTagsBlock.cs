﻿using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Model;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncXmldoc;

public class StreamLatestTagsBlock : TransformManyBlockBase<Tuple<SyncContext, SyncContext, SyncContext>, TagGraph, SyncContext>
{
    public StreamLatestTagsBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger)
        : base(parent, tokenSource, logger)
    {
    }

    public override IEnumerable<TagGraph> Transform(Tuple<SyncContext, SyncContext, SyncContext> input) => [Context.RubberduckDbMain, Context.RubberduckDbNext];
}
