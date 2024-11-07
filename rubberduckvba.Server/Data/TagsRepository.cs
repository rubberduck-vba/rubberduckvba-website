using Microsoft.Extensions.Options;
using rubberduckvba.Server.Model.Entity;

namespace rubberduckvba.Server.Data;

public class TagsRepository : Repository<TagEntity>
{
    public TagsRepository(IOptions<ConnectionSettings> settings)
        : base(settings) { }

    protected override string TableName { get; } = "Tags";
    protected override string SelectSql { get; } = @"
SELECT
    [Id],
    [DateTimeInserted],
    [DateTimeUpdated],
    [RepositoryId],
    [ReleaseId],
    [Name],
    [DateCreated],
    [InstallerDownloadUrl],
    [InstallerDownloads],
    [IsPreRelease]
FROM [dbo].[Tags]";

    protected override string InsertSql { get; } = @"
INSERT INTO [Tags] (
    [DateTimeInserted],
    [RepositoryId],
    [Name],
    [DateCreated],
    [InstallerDownloadUrl],
    [InstallerDownloads],
    [IsPreRelease])
VALUES (
    GETDATE(), 
    @repositoryId, 
    @name, 
    @dateCreated, 
    @installerDownloadUrl, 
    @installerDownloads, 
    @isPreRelease)";

    protected override string UpdateSql { get; } = @"
UPDATE [dbo].[Tags]
SET [DateTimeUpdated]=GETDATE(),
    [InstallerDownloads]=@installerDownloads
WHERE [Id]=@id";

}
