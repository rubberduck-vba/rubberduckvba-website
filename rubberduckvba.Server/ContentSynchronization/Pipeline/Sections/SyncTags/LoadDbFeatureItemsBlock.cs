using rubberduckvba.Server.ContentSynchronization.Pipeline.Abstract;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Data;
using rubberduckvba.Server.Model;
using rubberduckvba.Server.Model.Entity;
using rubberduckvba.Server.Services;

namespace rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.SyncTags;

public class LoadDbFeatureItemsBlock : ActionBlockBase<SyncRequestParameters, SyncContext>
{
    private IRubberduckDbService _db;
    private readonly IRepository<InspectionEntity> _inspections;
    private readonly IRepository<QuickFixEntity> _quickfixes;
    private readonly IRepository<AnnotationEntity> _annotations;

    public LoadDbFeatureItemsBlock(PipelineSection<SyncContext> parent, CancellationTokenSource tokenSource, ILogger logger,
        IRepository<InspectionEntity> inspections, IRepository<QuickFixEntity> quickfixes, IRepository<AnnotationEntity> annotations)
        : base(parent, tokenSource, logger)
    {
        _inspections = inspections;
        _quickfixes = quickfixes;
        _annotations = annotations;
    }

    protected override async Task ActionAsync(SyncRequestParameters input)
    {
        await Task.WhenAll([
            Task.Run(() => _inspections.GetAll()).ContinueWith(t => Context.LoadInspections(t.Result.Select(e => new Inspection(e)))),
            Task.Run(() => _quickfixes.GetAll()).ContinueWith(t => Context.LoadQuickFixes(t.Result.Select(e => new QuickFix(e)))),
            Task.Run(() => _annotations.GetAll()).ContinueWith(t => Context.LoadAnnotations(t.Result.Select(e => new Annotation(e))))
        ]);
    }
}