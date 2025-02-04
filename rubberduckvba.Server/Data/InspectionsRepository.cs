using Microsoft.Extensions.Options;
using rubberduckvba.Server.Model.Entity;

namespace rubberduckvba.Server.Data;

public class InspectionsRepository : Repository<InspectionEntity>, IRepository<InspectionEntity>
{
    public InspectionsRepository(IOptions<ConnectionSettings> settings)
        : base(settings) { }

    protected override string TableName { get; } = "Inspections";
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
    a.[InspectionType],
    a.[DefaultSeverity],
    a.[Summary],
    a.[Reasoning],
    a.[Remarks],
    a.[HostApp],
    a.[References],
    a.[QuickFixes],
    a.[JsonExamples],
    a.[IsNew],
    a.[IsDiscontinued],
    a.[IsHidden]
FROM [dbo].[Inspections] a
INNER JOIN [dbo].[Features] f ON a.[FeatureId]=f.[Id]";

    protected override string InsertSql { get; } = @"
INSERT INTO [dbo].[Inspections] (
    [DateTimeInserted],
    [FeatureId],
    [TagAssetId],
    [SourceUrl],
    [Name],
    [InspectionType],
    [DefaultSeverity],
    [Summary],
    [Reasoning],
    [Remarks],
    [HostApp],
    [References],
    [QuickFixes],
    [JsonExamples],
    [IsNew],
    [IsDiscontinued],
    [IsHidden])
VALUES (
    GETDATE(),
    @featureId,
    @tagAssetId,
    @sourceUrl,
    @name,
    @inspectionType,
    @defaultSeverity,
    @summary,
    @reasoning,
    @remarks,
    @hostApp,
    @references,
    @quickfixes,
    @jsonExamples,
    @isNew,
    @isDiscontinued,
    @isHidden)";

    protected override string UpdateSql { get; } = @"
UPDATE [dbo].[Inspections]
SET [DateTimeUpdated]=GETDATE(),
    [TagAssetId]=@tagAssetId,
    [SourceUrl]=@sourceUrl,
    [InspectionType]=@inspectionType,
    [DefaultSeverity]=@defaultSeverity,
    [Summary]=@summary,
    [Reasoning]=@reasoning,
    [Remarks]=@remarks,
    [HostApp]=@hostApp,
    [References]=@references,
    [QuickFixes]=@quickFixes,
    [JsonExamples]=@jsonExamples,
    [IsNew]=@isNew,
    [IsDiscontinued]=@isDiscontinued,
    [IsHidden]=@isHidden
WHERE [Id]=@id";

}
