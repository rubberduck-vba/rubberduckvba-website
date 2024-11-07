using Microsoft.Extensions.Options;
using rubberduckvba.Server.Model.Entity;

namespace rubberduckvba.Server.Data;

public class AnnotationsRepository : Repository<AnnotationEntity>
{
    public AnnotationsRepository(IOptions<ConnectionSettings> settings)
        : base(settings) { }

    protected override string TableName { get; } = "Annotations";
    protected override string SelectSql { get; } = @"
SELECT 
    [Id],
    [DateTimeInserted],
    [DateTimeUpdated],
    [FeatureId],
    [TagAssetId],
    [SourceUrl],
    [Name],
    [Remarks],
    [JsonParameters],
    [JsonExamples]
FROM [dbo].[Annotations]";

    protected override string InsertSql { get; } = @"
INSERT INTO [dbo].[Annotations] (
    [DateTimeInserted],
    [FeatureId],
    [TagAssetId],
    [SourceUrl],
    [Name],
    [Remarks],
    [JsonParameters],
    [JsonExamples])
VALUES (
    GETDATE(),
    @featureId,
    @tagAssetId,
    @sourceUrl,
    @name,
    @remarks,
    @jsonParameters,
    @jsonExamples)";

    protected override string UpdateSql { get; } = @"
UPDATE [dbo].[Annotations]
SET [DateTimeUpdated]=GETDATE(),
    [TagAssetId]=@tagAssetId,
    [Remarks]=@remarks,
    [JsonParameters]=@jsonParameters,
    [JsonExamples]=@jsonExamples
WHERE [Id]=@id";

}