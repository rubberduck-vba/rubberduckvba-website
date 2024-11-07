using Microsoft.Extensions.Options;
using rubberduckvba.Server.Model.Entity;

namespace rubberduckvba.Server.Data;

public class InspectionsRepository : Repository<InspectionEntity>
{
    public InspectionsRepository(IOptions<ConnectionSettings> settings)
        : base(settings) { }

    protected override string TableName { get; } = "Inspections";
    protected override string SelectSql { get; } = @"
SELECT 
    [Id],
    [DateTimeInserted],
    [DateTimeUpdated],
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
    [JsonExamples]
FROM [dbo].[Inspections]";

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
    [JsonExamples])
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
    @jsonExamples)";

    protected override string UpdateSql { get; } = @"
UPDATE [dbo].[Inspections]
SET [DateTimeUpdated]=GETDATE(),
    [TagAssetId]=@tagAssetId,
    [InspectionType]=@inspectionType,
    [DefaultSeverity]=@defaultSeverity,
    [Summary]=@summary,
    [Reasoning]=@reasoning,
    [Remarks]=@remarks,
    [HostApp]=@hostApp,
    [References]=@references,
    [QuickFixes]=@quickFixes,
    [JsonExamples]=@jsonExamples
WHERE [Id]=@id";

}
