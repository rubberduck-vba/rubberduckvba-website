using Microsoft.Extensions.Options;
using rubberduckvba.Server.Model.Entity;

namespace rubberduckvba.Server.Data;

public class FeaturesRepository : Repository<FeatureEntity>, IRepository<FeatureEntity>
{
    public FeaturesRepository(IOptions<ConnectionSettings> settings)
        : base(settings) { }

    protected override string TableName { get; } = "Features";
    protected override string? ParentFKColumnName { get; } = "ParentId";

    protected override string SelectSql { get; } = @"
SELECT 
    [Id],
    [DateTimeInserted],
    [DateTimeUpdated],
    [RepositoryId],
    [ParentId],
    [Name],
    [Title],
    [ShortDescription],
    [Description],
    [IsNew],
    [HasImage]
FROM [dbo].[Features]";

    protected override string InsertSql { get; } = @"
INSERT INTO [dbo].[Features] (
    [DateTimeInserted],
    [RepositoryId],
    [ParentId],
    [Name],
    [Title],
    [ShortDescription],
    [Description],
    [IsHidden],
    [IsNew],
    [HasImage]) 
VALUES (
    GETDATE(),
    @repositoryId,
    @parentId,
    @name,
    @title,
    @shortDescription,
    @description,
    @isHidden,
    @isNew,
    @hasImage)";

    protected override string UpdateSql { get; } = @"
UPDATE [dbo].[Features] SET
  [DateTimeUpdated]=GETDATE(),
  [RepositoryId]=@repositoryId,
  [ParentId]=@parentId,
  [Name]=@name,
  [Title]=@title,
  [ShortDescription]=@shortDescription,
  [Description]=@description,
  [IsHidden]=@isHidden,
  [IsNew]=@isNew,
  [HasImage]=@hasImage
WHERE [Id]=@id";

}
