using Microsoft.Extensions.Options;
using rubberduckvba.Server.Model.Entity;

namespace rubberduckvba.Server.Data;

public class AnnotationsRepository : Repository<AnnotationEntity>, IRepository<AnnotationEntity>
{
    public AnnotationsRepository(IOptions<ConnectionSettings> settings)
        : base(settings) { }

    protected override string TableName { get; } = "Annotations";
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
    a.[JsonParameters],
    a.[JsonExamples],
    a.[IsNew],
    a.[IsDiscontinued],
    a.[IsHidden]
FROM [dbo].[Annotations] a
INNER JOIN [dbo].[Features] f ON a.[FeatureId]=f.[Id]";

    protected override string InsertSql { get; } = @"
INSERT INTO [dbo].[Annotations] (
    [DateTimeInserted],
    [FeatureId],
    [TagAssetId],
    [SourceUrl],
    [Name],
    [Summary],
    [Remarks],
    [JsonParameters],
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
    @summary,
    @remarks,
    @jsonParameters,
    @jsonExamples,
    @isNew,
    @isDiscontinued,
    @isHidden)";

    protected override string UpdateSql { get; } = @"
UPDATE [dbo].[Annotations]
SET [DateTimeUpdated]=GETDATE(),
    [TagAssetId]=@tagAssetId,
    [Summary]=@summary,
    [Remarks]=@remarks,
    [JsonParameters]=@jsonParameters,
    [JsonExamples]=@jsonExamples,
    [IsNew]=@isNew,
    [IsDiscontinued]=@isDiscontinued,
    [IsHidden]=@isHidden
WHERE [Id]=@id";

}