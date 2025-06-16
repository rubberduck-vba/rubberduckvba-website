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
    a.[Id],
    a.[DateTimeInserted],
    a.[DateTimeUpdated],
    a.[RepositoryId],
    a.[ParentId],
    f.[Name] AS [FeatureName],
    a.[Name],
    a.[Title],
    a.[ShortDescription],
    a.[Description],
    a.[IsNew],
    a.[HasImage],
    a.[Links]
FROM [dbo].[Features] a
LEFT JOIN [dbo].[Features] f ON a.[ParentId]=f.[Id]";

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
