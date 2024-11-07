using Microsoft.Extensions.Options;
using rubberduckvba.Server.Model.Entity;

namespace rubberduckvba.Server.Data;

public class QuickFixRepository : Repository<QuickFixEntity>
{
    public QuickFixRepository(IOptions<ConnectionSettings> settings)
        : base(settings) { }
    protected override string TableName { get; } = "QuickFixes";

    protected override string SelectSql { get; } = @"
SELECT 
    [Id],
    [DateTimeInserted],
    [DateTimeUpdated],
    [FeatureId],
    [TagAssetId],
    [SourceUrl],
    [Name],
    [Summary],
    [Remarks],
    [CanFixMultiple],
    [CanFixProcedure],
    [CanFixModule],
    [CanFixProject],
    [CanFixAll],
    [Inspections],
    [JsonExamples]
FROM [dbo].[QuickFixes]";

    protected override string InsertSql { get; } = @"
INSERT INTO [dbo].[QuickFixes] (
    [DateTimeInserted],
    [FeatureId],
    [TagAssetId],
    [SourceUrl],
    [Name],
    [Summary],
    [Remarks],
    [CanFixMultiple],
    [CanFixProcedure],
    [CanFixModule],
    [CanFixProject],
    [CanFixAll],
    [Inspections],
    [JsonExamples]
VALUES (
    GETDATE(),
    @featureId,
    @tagAssetId,
    @sourceUrl,
    @name,
    @summary,
    @remarks,
    @canFixMultiple,
    @canFixProcedure,
    @canFixModule,
    @canFixProject,
    @canFixAll,
    @inspections,
    @jsonExamples)";

    protected override string UpdateSql { get; } = @"
UPDATE [dbo].[QuickFixes]
SET [DateTimeUpdated]=GETDATE(),
    [TagAssetId]=@tagAssetId,
    [Summary]=@summary,
    [Remarks]=@remarks,
    [CanFixMultiple]=@canFixMultiple,
    [CanFixProcedure]=@canFixProcedure,
    [CanFixModule]=@canFixModule,
    [CanFixProject]=@canFixProject,
    [CanFixAll]=@canFixAll,
    [JsonExamples]=@jsonExamples
WHERE [Id]=@id";

}