using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using rubberduckvba.com.Server.ContentSynchronization.Pipeline;
using rubberduckvba.com.Server.Data;
using System.Collections.Immutable;
using System.Diagnostics;

namespace rubberduckvba.com.Server.Services;

public interface IStagingServices
{
    Task StageAsync(StagingContext staging, CancellationToken token);
}

public class StagingServices(IRubberduckDbService service) : IStagingServices
{
    public async Task StageAsync(StagingContext context, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var updatedTags = context.ProcessedTags.Where(e => e.Id != default);
        await service.UpdateAsync(updatedTags);

        token.ThrowIfCancellationRequested();
        var newTags = context.ProcessedTags.Where(e => e.Id == default);
        await service.CreateAsync(newTags, context.Parameters.RepositoryId);

        token.ThrowIfCancellationRequested();
        await service.UpdateAsync(context.UpdatedFeatureItems);

        token.ThrowIfCancellationRequested();
        await service.CreateAsync(context.NewFeatureItems);
    }
}

public interface ISynchronizationService
{
    Task LogAsync(SynchronizationRequest synchronization);
}

public enum RepositoryId
{
    Rubberduck = 1,
    Rubberduck3 = 2
}

public interface IRubberduckDbService
{
    Task<IEnumerable<Tag>> GetAllTagsAsync();

    Task<IEnumerable<Tag>> GetLatestTagsAsync(RepositoryId repositoryId);
    Task<TagGraph> GetLatestTagAsync(RepositoryId repositoryId, bool includePreRelease);

    Task UpdateAsync(IEnumerable<Tag> tags);
    Task UpdateAsync(IEnumerable<FeatureXmlDoc> featureItems);

    Task CreateAsync(IEnumerable<TagGraph> tags, RepositoryId repositoryId);
    Task CreateAsync(IEnumerable<FeatureXmlDoc> featureItems);

    Task<IEnumerable<Feature>> GetTopLevelFeatures(RepositoryId? repositoryId = default);
    Task<FeatureGraph> ResolveFeature(RepositoryId repositoryId, string name);
    Task<int?> GetFeatureId(RepositoryId repositoryId, string name);
    Task<IEnumerable<FeatureXmlDoc>> GetXmlDocFeaturesAsync(RepositoryId repositoryId);
    Task<Feature> SaveFeature(Feature feature);
}

public class SynchronizationDbService : ISynchronizationService
{
    private readonly string _connectionString;

    public SynchronizationDbService(IOptions<ConnectionSettings> settings, ILogger<ServiceLogger> logger)
    {
        _connectionString = settings.Value.RubberduckDb ?? throw new InvalidOperationException();
        Logger = logger;
    }

    private ILogger Logger { get; }

    public async Task LogAsync(SynchronizationRequest synchronization)
    {
        const string insertSql = @"
INSERT INTO [SynchronizationRequests] ([DateTimeInserted],[RequestId],[Status],[Message])
VALUES (@ts, @requestId, @status, @message);";
        const string updateSql = @"
UPDATE [SynchronizationRequests] SET
    [DateTimeUpdated]=@ts,
    [UtcDateTimeStarted]=@utcStart,
    [UtcDateTimeEnded]=@utcEnd,
    [Status]=@status,
    [Message]=@message
WHERE [RequestId]=@requestId;
";
        using var db = new SqlConnection(_connectionString);
        await db.OpenAsync();

        using var transaction = await db.BeginTransactionAsync();

        if (synchronization.Status == default)
        {
            await db.ExecuteAsync(insertSql, new
            {
                ts = TimeProvider.System.GetUtcNow().ToTimestampString(),
                requestId = synchronization.RequestId,
                status = (int)synchronization.Status,
                message = synchronization.Message,
            }, transaction);
        }
        else
        {
            await db.ExecuteAsync(updateSql, new
            {
                ts = TimeProvider.System.GetUtcNow().ToTimestampString(),
                requestId = synchronization.RequestId,
            }, transaction);
        }

        await transaction.CommitAsync();
    }
}

public class RubberduckDbService : IRubberduckDbService
{
    private readonly string _connectionString;

    public RubberduckDbService(IOptions<ConnectionSettings> settings, ILogger<ServiceLogger> logger)
    {
        _connectionString = settings.Value.RubberduckDb ?? throw new InvalidOperationException("ConnectionString 'RubberduckDb' could not be retrieved.");
        Logger = logger;
    }

    private ILogger Logger { get; }

    private async Task<SqlConnection> GetDbConnection()
    {
        var db = new SqlConnection(_connectionString);
        await db.OpenAsync();

        return db;
    }

    public async Task<IEnumerable<Tag>> GetAllTagsAsync()
    {
        const string sql = @"
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
FROM [Tags]
";
        using var db = await GetDbConnection();

        var sw = Stopwatch.StartNew();
        var result = (await db.QueryAsync<Tag>(sql)).ToArray();
        sw.Stop();

        Logger.LogInformation(nameof(GetAllTagsAsync) + " | SELECT operation completed ({results}) | ⏱️ {elapsed}", result.Length, sw.Elapsed);
        return result;
    }

    public async Task<IEnumerable<Feature>> GetTopLevelFeatures(RepositoryId? repositoryId = default)
    {
        const string sql = @"
SELECT 
    [Id],
    [DateTimeInserted],
    [DateTimeUpdated],
    [RepositoryId],
    [Name],
    [Title],
    [ShortDescription],
    [IsNew],
    [HasImage]
FROM [Features] 
WHERE [RepositoryId]=ISNULL(@repositoryId,[RepositoryId])
AND [ParentId] IS NULL
AND [IsHidden]=0;
";
        using var db = await GetDbConnection();
        var parameters = new { repositoryId = (int)(repositoryId ?? RepositoryId.Rubberduck) };

        var sw = Stopwatch.StartNew();
        var result = (await db.QueryAsync<Feature>(sql, parameters)).ToArray();
        sw.Stop();

        Logger.LogInformation(nameof(GetTopLevelFeatures) + " | SELECT operation completed ({results}) | ⏱️ {elapsed}", result.Length, sw.Elapsed);
        return result;
    }

    public async Task<FeatureGraph> ResolveFeature(RepositoryId repositoryId, string name)
    {
        const string featureSql = @"
WITH feature AS (
    SELECT [Id] 
    FROM [Features] 
    WHERE [RepositoryId]=@repositoryId AND LOWER([Name])=LOWER(@name)
)
SELECT 
    src.[Id],
    src.[ParentId],
    src.[DateTimeInserted],
    src.[DateTimeUpdated],
    src.[Name],
    src.[Title],
    src.[ShortDescription],
    src.[Description],
    src.[IsNew],
    src.[HasImage] 
FROM [Features] src 
INNER JOIN feature ON src.[Id]=feature.[Id] OR src.[ParentId]=feature.[Id];
";
        const string itemSql = @"
WITH feature AS (
    SELECT [Id] 
    FROM [Features] 
    WHERE [RepositoryId]=@repositoryId AND LOWER([Name])=LOWER(@name)
)
SELECT 
    src.[Id],
    src.[DateTimeInserted],
    src.[DateTimeUpdated],
    src.[FeatureId],
    f.[Name] AS [FeatureName],
    f.[Title] AS [FeatureTitle],
    src.[Name],
    src.[Title],
    src.[Description] AS [Summary],
    src.[IsNew],
    src.[IsDiscontinued],
    src.[IsHidden],
    src.[TagAssetId],
    t.[Id] AS [TagId],
    src.[SourceUrl],
    src.[Serialized],
    t.[Name] AS [TagName]
FROM [FeatureXmlDoc] src
INNER JOIN feature ON src.[FeatureId]=feature.[Id]
INNER JOIN [Features] f ON feature.[Id]=f.[Id]
INNER JOIN [TagAssets] a ON src.[TagAssetId]=a.[Id]
INNER JOIN [Tags] t ON a.[TagId]=t.[Id]
ORDER BY src.[IsNew] DESC, src.[IsDiscontinued] DESC, src.[Name];
";

        using var db = await GetDbConnection();
        var sw = Stopwatch.StartNew();

        var parameters = new { repositoryId, name };
        var features = await db.QueryAsync<Feature>(featureSql, parameters);
        var materialized = features.ToArray();

        var items = await db.QueryAsync<FeatureXmlDoc>(itemSql, parameters);

        var graph = materialized.Where(e => e.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
            .Select(e => FeatureGraph.FromFeature(e) with
            {
                Features = materialized.Where(f => f.ParentId == e.Id).ToImmutableArray(),
                Items = items.ToImmutableArray()
            }).Single();
        sw.Stop();

        Logger.LogInformation(nameof(ResolveFeature) + " | All SELECT operations completed | ⏱️ {elapsed}", sw.Elapsed);
        return graph;
    }

    public async Task<Feature> SaveFeature(Feature feature)
    {
        const string insertSql = @"
INSERT INTO [Features] ([DateTimeInserted],[RepositoryId],[ParentId],[Name],[Title],[ShortDescription],[Description],[IsHidden],[IsNew],[HasImage]) 
VALUES (@ts,@repositoryId,@parentId,@name,@title,@shortDescription,@description,@isHidden,@isNew,@hasImage) 
RETURNING [Id];
";
        const string updateSql = @"
UPDATE [Features] SET
  [DateTimeUpdated]=@ts,
  [RepositoryId]=@repositoryId,
  [ParentId]=@parentId,
  [Name]=@name,
  [Title]=@title,
  [ShortDescription]=@shortDescription,
  [Description]=@description,
  [IsHidden]=@isHidden,
  [IsNew]=@isNew,
  [HasImage]=@hasImage
WHERE [Id]=@id;
";
        Feature result;

        using var db = await GetDbConnection();
        using var transaction = await db.BeginTransactionAsync();

        if (feature.Id == default)
        {
            var parameters = new
            {
                ts = TimeProvider.System.GetUtcNow().ToTimestampString(),
                repositoryId = feature.RepositoryId,
                parentId = feature.ParentId,
                name = feature.Name,
                shortDescription = feature.ShortDescription,
                description = feature.Description,
                isHidden = feature.IsHidden,
                isNew = feature.IsNew,
                hasImage = feature.HasImage,
            };
            var id = await db.ExecuteAsync(insertSql, parameters, transaction);
            result = feature with { Id = id };
        }
        else
        {
            var parameters = new
            {
                ts = TimeProvider.System.GetUtcNow().ToTimestampString(),
                repositoryId = feature.RepositoryId,
                parentId = feature.ParentId,
                name = feature.Name,
                shortDescription = feature.ShortDescription,
                description = feature.Description,
                isHidden = feature.IsHidden,
                isNew = feature.IsNew,
                hasImage = feature.HasImage,
                id = feature.Id
            };
            await db.ExecuteAsync(updateSql, parameters, transaction);
            result = feature;
        }

        var trx = Stopwatch.StartNew();
        await transaction.CommitAsync();
        trx.Stop();
        Logger.LogInformation(nameof(SaveFeature) + " | Transaction committed | ⏱️ {elapsed}", trx.Elapsed);
        return result;
    }

    public async Task CreateAsync(IEnumerable<TagGraph> tags, RepositoryId repositoryId)
    {
        if (!tags.Any())
        {
            return;
        }

        const string tagSql = @"
INSERT INTO [Tags] ([DateTimeInserted],[RepositoryId],[Name],[DateCreated],[InstallerDownloadUrl],[InstallerDownloads],[IsPreRelease])
VALUES (@ts, @repositoryId, @name, @dateCreated, @installerDownloadUrl, @installerDownloads, @isPreRelease)
RETURNING [Id];";

        const string assetSql = @"
INSERT INTO [TagAssets] ([DateTimeInserted],[TagId],[Name],[DownloadUrl])
VALUES (@ts, @tagId, @name, @downloadUrl);
";

        using var db = await GetDbConnection();
        using var transaction = db.BeginTransaction();

        foreach (var tag in tags)
        {
            var tagInfo = new
            {
                ts = TimeProvider.System.GetUtcNow().ToTimestampString(),
                repositoryId = (int)repositoryId,
                name = tag.Name,
                dateCreated = tag.DateCreated,
                installerDownloadUrl = tag.InstallerDownloadUrl,
                installerDownloads = tag.InstallerDownloads,
                isPreRelease = tag.IsPreRelease
            };
            await db.ExecuteAsync(tagSql, tagInfo, transaction);
            var id = await db.ExecuteScalarAsync<int>("SELECT [Id] FROM [Tags] WHERE [Name]=@name;", new { name = tag.Name });

            foreach (var asset in tag.Assets)
            {
                var assetInfo = new
                {
                    ts = TimeProvider.System.GetUtcNow().ToTimestampString(),
                    tagId = id,
                    name = asset.Name,
                    downloadUrl = asset.DownloadUrl
                };
                await db.ExecuteAsync(assetSql, assetInfo, transaction);
            }
        }

        var trx = Stopwatch.StartNew();
        await transaction.CommitAsync();
        trx.Stop();
        Logger.LogInformation(nameof(CreateAsync) + " (" + nameof(TagGraph) + ") | Transaction committed | ⏱️ {elapsed}", trx.Elapsed);
    }

    public async Task<IEnumerable<Tag>> GetLatestTagsAsync(RepositoryId repositoryId)
    {
        const string sql = @"
WITH t AS (
    SELECT [Id], RANK() OVER (PARTITION BY src.[IsPreRelease] ORDER BY src.[DateCreated] DESC) AS [Rank]
    FROM [Tags] src
    WHERE [RepositoryId]=@repositoryId
)
SELECT 
    src.[Id],
    src.[DateTimeInserted],
    src.[DateTimeUpdated],
    src.[Name],
    src.[DateCreated],
    src.[InstallerDownloadUrl],
    src.[InstallerDownloads],
    src.[IsPreRelease] 
FROM [Tags] src 
INNER JOIN t ON src.[Id]=t.[Id]
WHERE t.[Rank]=1;";

        using var db = await GetDbConnection();
        var sw = Stopwatch.StartNew();
        var results = await db.QueryAsync<Tag>(sql, new { repositoryId = (int)repositoryId });

        Logger.LogInformation(nameof(GetLatestTagsAsync) + " | SELECT operation completed | ⏱️ {elapsed}", sw.Elapsed);
        return results;
    }

    public async Task<TagGraph> GetLatestTagAsync(RepositoryId repositoryId, bool preRelease)
    {
        const string tagSql = @"
WITH t AS (
    SELECT [Id], RANK() OVER (PARTITION BY src.[IsPreRelease] ORDER BY src.[DateCreated] DESC) AS [Rank]
    FROM [Tags] src
    WHERE [RepositoryId]=@repositoryId
)
SELECT 
    src.[Id],
    src.[DateTimeInserted],
    src.[DateTimeUpdated],
    src.[Name],
    src.[DateCreated],
    src.[InstallerDownloadUrl],
    src.[InstallerDownloads],
    src.[IsPreRelease] 
FROM [Tags] src 
INNER JOIN t ON src.[Id]=t.[Id]
WHERE t.[Rank]=1 AND src.[IsPreRelease]=@pre
";
        const string assetsSql = @"
SELECT 
    src.[Id],
    src.[DateTimeInserted],
    src.[DateTimeUpdated],
    src.[TagId],
    src.[Name], 
    src.[DownloadUrl]
FROM [TagAssets] src
WHERE src.[TagId]=@id
";

        using var db = await GetDbConnection();
        var sw = Stopwatch.StartNew();

        var tag = await db.QuerySingleAsync<Tag>(tagSql, new { repositoryId = (int)repositoryId, pre = preRelease });
        var assets = await db.QueryAsync<TagAsset>(assetsSql, new { id = tag.Id });

        Logger.LogInformation(nameof(GetLatestTagAsync) + " | All SELECT operations completed | ⏱️ {elapsed}", sw.Elapsed);

        return new TagGraph
        {
            Id = tag.Id,
            DateTimeInserted = tag.DateTimeInserted,
            DateTimeUpdated = tag.DateTimeUpdated,
            DateCreated = tag.DateCreated,
            ReleaseId = tag.ReleaseId,
            IsPreRelease = tag.IsPreRelease,
            Name = tag.Name,
            InstallerDownloadUrl = tag.InstallerDownloadUrl,
            InstallerDownloads = tag.InstallerDownloads,
            Assets = assets
        };
    }

    public async Task UpdateAsync(IEnumerable<Tag> tags)
    {
        if (!tags.Any())
        {
            return;
        }

        const string sql = @"
UPDATE [Tags] SET 
  [DateTimeUpdated]=@ts,
  [InstallerDownloads]=@installerDownloads
WHERE [Id]=@id;";

        using var db = await GetDbConnection();
        var sw = Stopwatch.StartNew();
        using var transaction = await db.BeginTransactionAsync();

        foreach (var tag in tags)
        {
            var args = new
            {
                ts = TimeProvider.System.GetUtcNow().ToTimestampString(),
                id = tag.Id,
                installerDownloads = tag.InstallerDownloads
            };
            await db.ExecuteAsync(sql, args, transaction);
        }

        await transaction.CommitAsync();
        sw.Stop();

        Logger.LogInformation(nameof(UpdateAsync) + "(" + nameof(Tag) + ") | Transaction committed; all UPDATE operations completed | ⏱️ {elapsed}", sw.Elapsed);
    }

    public async Task<int?> GetFeatureId(RepositoryId repositoryId, string name)
    {
        const string sql = "SELECT [Id] FROM [Features] WHERE [RepositoryId]=@repositoryId AND [Name]=@name;";

        using var db = await GetDbConnection();
        var sw = Stopwatch.StartNew();

        var id = await db.ExecuteScalarAsync<int?>(sql, new { repositoryId, name });
        Logger.LogInformation(nameof(GetFeatureId) + " | SELECT operation completed | ⏱️ {elapsed}", sw.Elapsed);

        return id;
    }

    public async Task<IEnumerable<FeatureXmlDoc>> GetXmlDocFeaturesAsync(RepositoryId repositoryId)
    {
        const string sql = @"
SELECT
    x.[Id],
    x.[DateTimeInserted],
    x.[DateTimeUpdated],
    x.[FeatureId],
    x.[Name],
    x.[Title],
    x.[Description],
    x.[IsNew],
    x.[IsDiscontinued],
    x.[IsHidden],
    x.[TagAssetId],
    x.[SourceUrl],
    x.[Serialized]
FROM [FeatureXmlDoc] AS x 
INNER JOIN [Features] AS f ON x.[FeatureId]=f.[Id]
WHERE f.[RepositoryId]=@repositoryId";

        using var db = await GetDbConnection();
        var sw = Stopwatch.StartNew();

        var result = await db.QueryAsync<FeatureXmlDoc>(sql, new { repositoryId });
        Logger.LogInformation(nameof(GetXmlDocFeaturesAsync) + " | SELECT operation completed | ⏱️ {elapsed}", sw.Elapsed);

        return result;
    }

    public async Task UpdateAsync(IEnumerable<FeatureXmlDoc> featureItems)
    {
        const string updateSql = @"
UPDATE [FeatureXmlDoc]
SET [DateTimeUpdated]=@ts,
    [Title]=@title,
    [Description]=@description,
    [IsNew]=@isNew,
    [IsDiscontinued]=@isDiscontinued,
    [IsHidden]=@isHidden,
    [TagAssetId]=@tagAssetId,
    [SourceUrl]=@sourceUrl,
    [Serialized]=@serialized
WHERE [Id]=@id";

        if (!featureItems.Any())
        {
            return;
        }

        using var db = await GetDbConnection();
        using var transaction = db.BeginTransaction();
        var sw = Stopwatch.StartNew();

        var timestamp = TimeProvider.System.GetUtcNow().ToTimestampString();
        foreach (var update in featureItems)
        {
            await db.ExecuteAsync(updateSql, new
            {
                ts = timestamp,
                title = update.Title,
                description = update.Summary,
                isNew = update.IsNew,
                isDiscontinued = update.IsDiscontinued,
                isHidden = update.IsHidden,
                tagAssetId = update.TagAssetId,
                sourceUrl = update.SourceUrl,
                serialized = update.Serialized,
                id = update.Id,
            }, transaction);
        }

        await transaction.CommitAsync();
        Logger.LogInformation(nameof(UpdateAsync) + "(" + nameof(FeatureXmlDoc) + ") | Transaction committed; all UPDATE operations completed | ⏱️ {elapsed}", sw.Elapsed);
    }

    public async Task CreateAsync(IEnumerable<FeatureXmlDoc> featureItems)
    {
        const string insertSql = @"
INSERT INTO [FeatureXmlDoc] ([DateTimeInserted],[FeatureId],[Name],[Title],[Description],[IsNew],[IsDiscontinued],[IsHidden],[TagAssetId],[SourceUrl],[Serialized])
VALUES (@ts,@featureId,@name,@title,@description,@isNew,@isDiscontinued,@isHidden,@tagAssetId,@sourceUrl,@serialized);
";

        if (!featureItems.Any())
        {
            return;
        }

        using var db = await GetDbConnection();
        using var transaction = db.BeginTransaction();
        var sw = Stopwatch.StartNew();

        var timestamp = TimeProvider.System.GetUtcNow().ToTimestampString();
        foreach (var insert in featureItems)
        {
            await db.ExecuteAsync(insertSql, new
            {
                ts = timestamp,
                featureId = insert.FeatureId,
                name = insert.Name,
                title = insert.Title,
                description = insert.Summary,
                isNew = insert.IsNew,
                isDiscontinued = insert.IsDiscontinued,
                isHidden = insert.IsHidden,
                tagAssetId = insert.TagAssetId,
                sourceUrl = insert.SourceUrl,
                serialized = insert.Serialized
            }, transaction);
        }

        await transaction.CommitAsync();
        Logger.LogInformation(nameof(CreateAsync) + "(" + nameof(FeatureXmlDoc) + ") | Transaction committed; all INSERT operations completed | ⏱️ {elapsed}", sw.Elapsed);
    }
}