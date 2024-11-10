using Microsoft.Extensions.Options;
using rubberduckvba.Server;
using rubberduckvba.Server.Model.Entity;

namespace rubberduckvba.Server.Data;

public class TagAssetsRepository : Repository<TagAssetEntity>
{
    public TagAssetsRepository(IOptions<ConnectionSettings> settings)
        : base(settings) { }

    protected override string TableName { get; } = "TagAssets";
    protected override string? ParentFKColumnName => "TagId";

    protected override string SelectSql { get; } = @"
SELECT
    [Id],
    [DateTimeInserted],
    [DateTimeUpdated],
    [TagId],
    [Name],
    [DownloadUrl]
FROM [dbo].[TagAssets]";

    protected override string InsertSql { get; } = @"
INSERT INTO [dbo].[TagAssets] (
    [DateTimeInserted],
    [TagId],
    [Name],
    [DownloadUrl])
VALUES (
    GETDATE(), 
    @tagId, 
    @name,
    @downloadUrl);";

    protected override string UpdateSql => throw new NotImplementedException();
}