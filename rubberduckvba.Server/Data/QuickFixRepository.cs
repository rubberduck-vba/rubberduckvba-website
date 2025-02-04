using Microsoft.Extensions.Options;
using rubberduckvba.Server.Model.Entity;

namespace rubberduckvba.Server.Data;

public class QuickFixRepository : Repository<QuickFixEntity>, IRepository<QuickFixEntity>
{
    public QuickFixRepository(IOptions<ConnectionSettings> settings)
        : base(settings) { }
    protected override string TableName { get; } = "QuickFixes";

    protected override string SelectSql { get; } = @"
SELECT 
    a.[Id],
    a.[DateTimeInserted],
    a.[DateTimeUpdated],
    a.[FeatureId],
    f.[Name] AS [FeatureName],
    a.[TagAssetId],
    a.[SourceUrl],
    a.[Name],
    a.[Summary],
    a.[Remarks],
    a.[CanFixMultiple],
    a.[CanFixProcedure],
    a.[CanFixModule],
    a.[CanFixProject],
    a.[CanFixAll],
    a.[Inspections],
    a.[JsonExamples],
    a.[IsNew],
    a.[IsDiscontinued]
FROM [dbo].[QuickFixes] a
INNER JOIN [dbo].[Features] f ON a.[FeatureId]=f.[Id]";

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
    [JsonExamples],
    [IsNew],
    [IsDiscontinued])
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
    @jsonExamples,
    @isNew,
    @isDiscontinued)";

    protected override string UpdateSql { get; } = @"
UPDATE [dbo].[QuickFixes]
SET [DateTimeUpdated]=GETDATE(),
    [TagAssetId]=@tagAssetId,
    [SourceUrl]=@sourceUrl,
    [Summary]=@summary,
    [Remarks]=@remarks,
    [CanFixMultiple]=@canFixMultiple,
    [CanFixProcedure]=@canFixProcedure,
    [CanFixModule]=@canFixModule,
    [CanFixProject]=@canFixProject,
    [CanFixAll]=@canFixAll,
    [JsonExamples]=@jsonExamples,
    [IsNew]=@isNew,
    [IsDiscontinued]=@isDiscontinued
WHERE [Id]=@id";

}