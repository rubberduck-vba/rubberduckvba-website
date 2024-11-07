using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Model;
using rubberduckvba.Server.Services.rubberduckdb;

namespace rubberduckvba.Server.Services;

public interface IStagingServices
{
    Task StageAsync(StagingContext staging, CancellationToken token);
}

public class StagingServices(IRubberduckDbService service, TagServices tagService, FeatureServices featureServices) : IStagingServices
{
    public async Task StageAsync(StagingContext context, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var updatedTags = context.ProcessedTags.Where(e => e.Id != default);
        tagService.Update(updatedTags);

        token.ThrowIfCancellationRequested();
        var newTags = context.ProcessedTags.Where(e => e.Id == default);
        tagService.Create(newTags);

        //token.ThrowIfCancellationRequested();
        //featureServices.Update(context.UpdatedFeatureItems);
        //await service.UpdateAsync(context.UpdatedFeatureItems);

        //token.ThrowIfCancellationRequested();
        //await service.CreateAsync(context.NewFeatureItems);
    }
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
    Task CreateAsync(IEnumerable<TagGraph> tags, RepositoryId repositoryId);

    Task<IEnumerable<Feature>> GetTopLevelFeatures(RepositoryId? repositoryId = default);
    Task<FeatureGraph> ResolveFeature(RepositoryId repositoryId, string name);
    Task<int?> GetFeatureId(RepositoryId repositoryId, string name);
    Task<Feature> SaveFeature(Feature feature);
}

public class RubberduckDbService : IRubberduckDbService
{
    private readonly string _connectionString;
    private readonly TagServices _tagServices;
    private readonly FeatureServices _featureServices;

    public RubberduckDbService(IOptions<ConnectionSettings> settings, ILogger<ServiceLogger> logger,
        TagServices tagServices, FeatureServices featureServices)
    {
        _connectionString = settings.Value.RubberduckDb ?? throw new InvalidOperationException("ConnectionString 'RubberduckDb' could not be retrieved.");
        Logger = logger;

        _tagServices = tagServices;
        _featureServices = featureServices;
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
        return _tagServices.GetAllTags();
        //        const string sql = @"
        //SELECT
        //    [Id],
        //    [DateTimeInserted],
        //    [DateTimeUpdated],
        //    [RepositoryId],
        //    [ReleaseId],
        //    [Name],
        //    [DateCreated],
        //    [InstallerDownloadUrl],
        //    [InstallerDownloads],
        //    [IsPreRelease]
        //FROM [Tags]
        //";
        //        using var db = await GetDbConnection();

        //        var sw = Stopwatch.StartNew();
        //        var result = (await db.QueryAsync<Tag>(sql)).ToArray();
        //        sw.Stop();

        //        Logger.LogInformation(nameof(GetAllTagsAsync) + " | SELECT operation completed ({results}) | ⏱️ {elapsed}", result.Length, sw.Elapsed);
        //        return result;
    }

    public async Task<IEnumerable<Feature>> GetTopLevelFeatures(RepositoryId? repositoryId = default)
    {
        return _featureServices.Get(topLevelOnly: true);
        //        const string sql = @"
        //SELECT 
        //    [Id],
        //    [DateTimeInserted],
        //    [DateTimeUpdated],
        //    [RepositoryId],
        //    [Name],
        //    [Title],
        //    [ShortDescription],
        //    [IsNew],
        //    [HasImage]
        //FROM [Features] 
        //WHERE [RepositoryId]=ISNULL(@repositoryId,[RepositoryId])
        //AND [ParentId] IS NULL
        //AND [IsHidden]=0;
        //";
        //        using var db = await GetDbConnection();
        //        var parameters = new { repositoryId = (int)(repositoryId ?? RepositoryId.Rubberduck) };

        //        var sw = Stopwatch.StartNew();
        //        var result = (await db.QueryAsync<Feature>(sql, parameters)).ToArray();
        //        sw.Stop();

        //        Logger.LogInformation(nameof(GetTopLevelFeatures) + " | SELECT operation completed ({results}) | ⏱️ {elapsed}", result.Length, sw.Elapsed);
        //        return result;
    }

    public async Task<FeatureGraph> ResolveFeature(RepositoryId repositoryId, string name)
    {
        return _featureServices.Get(name);
        //        const string featureSql = @"
        //WITH feature AS (
        //    SELECT [Id] 
        //    FROM [Features] 
        //    WHERE [RepositoryId]=@repositoryId AND LOWER([Name])=LOWER(@name)
        //)
        //SELECT 
        //    src.[Id],
        //    src.[ParentId],
        //    src.[DateTimeInserted],
        //    src.[DateTimeUpdated],
        //    src.[Name],
        //    src.[Title],
        //    src.[ShortDescription],
        //    src.[Description],
        //    src.[IsNew],
        //    src.[HasImage] 
        //FROM [Features] src 
        //INNER JOIN feature ON src.[Id]=feature.[Id] OR src.[ParentId]=feature.[Id];
        //";
        //        const string itemSql = @"
        //WITH feature AS (
        //    SELECT [Id] 
        //    FROM [Features] 
        //    WHERE [RepositoryId]=@repositoryId AND LOWER([Name])=LOWER(@name)
        //)
        //SELECT 
        //    src.[Id],
        //    src.[DateTimeInserted],
        //    src.[DateTimeUpdated],
        //    src.[FeatureId],
        //    f.[Name] AS [FeatureName],
        //    f.[Title] AS [FeatureTitle],
        //    src.[Name],
        //    src.[Title],
        //    src.[Description] AS [Summary],
        //    src.[IsNew],
        //    src.[IsDiscontinued],
        //    src.[IsHidden],
        //    src.[TagAssetId],
        //    t.[Id] AS [TagId],
        //    src.[SourceUrl],
        //    src.[Serialized],
        //    t.[Name] AS [TagName]
        //FROM [FeatureXmlDoc] src
        //INNER JOIN feature ON src.[FeatureId]=feature.[Id]
        //INNER JOIN [Features] f ON feature.[Id]=f.[Id]
        //INNER JOIN [TagAssets] a ON src.[TagAssetId]=a.[Id]
        //INNER JOIN [Tags] t ON a.[TagId]=t.[Id]
        //ORDER BY src.[IsNew] DESC, src.[IsDiscontinued] DESC, src.[Name];
        //";

        //        using var db = await GetDbConnection();
        //        var sw = Stopwatch.StartNew();

        //        var parameters = new { repositoryId, name };
        //        var features = await db.QueryAsync<Feature>(featureSql, parameters);
        //        var materialized = features.ToArray();

        //        var items = await db.QueryAsync<FeatureXmlDoc>(itemSql, parameters);

        //        var graph = materialized.Where(e => e.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
        //            .Select(e => new FeatureGraph(e.ToEntity()) with
        //            {
        //                Features = materialized.Where(f => f.ParentId == e.Id).ToImmutableArray(),
        //                Items = items.ToImmutableArray()
        //            }).Single();
        //        sw.Stop();

        //        Logger.LogInformation(nameof(ResolveFeature) + " | All SELECT operations completed | ⏱️ {elapsed}", sw.Elapsed);
        //        return graph;
    }

    public async Task<Feature> SaveFeature(Feature feature)
    {
        if (feature.Id == default)
        {
            _featureServices.Insert(new FeatureGraph(feature.ToEntity()));
        }
        else
        {
            _featureServices.Update(new FeatureGraph(feature.ToEntity()));
        }
        // TODO return with id
        return feature;
        //        const string insertSql = @"
        //INSERT INTO [Features] ([DateTimeInserted],[RepositoryId],[ParentId],[Name],[Title],[ShortDescription],[Description],[IsHidden],[IsNew],[HasImage]) 
        //VALUES (@ts,@repositoryId,@parentId,@name,@title,@shortDescription,@description,@isHidden,@isNew,@hasImage) 
        //RETURNING [Id];
        //";
        //        const string updateSql = @"
        //UPDATE [Features] SET
        //  [DateTimeUpdated]=@ts,
        //  [RepositoryId]=@repositoryId,
        //  [ParentId]=@parentId,
        //  [Name]=@name,
        //  [Title]=@title,
        //  [ShortDescription]=@shortDescription,
        //  [Description]=@description,
        //  [IsHidden]=@isHidden,
        //  [IsNew]=@isNew,
        //  [HasImage]=@hasImage
        //WHERE [Id]=@id;
        //";
        //        Feature result;

        //        using var db = await GetDbConnection();
        //        using var transaction = await db.BeginTransactionAsync();

        //        if (feature.Id == default)
        //        {
        //            var parameters = new
        //            {
        //                ts = TimeProvider.System.GetUtcNow().ToTimestampString(),
        //                repositoryId = feature.RepositoryId,
        //                parentId = feature.ParentId,
        //                name = feature.Name,
        //                shortDescription = feature.ShortDescription,
        //                description = feature.Description,
        //                isHidden = feature.IsHidden,
        //                isNew = feature.IsNew,
        //                hasImage = feature.HasImage,
        //            };
        //            var id = await db.ExecuteAsync(insertSql, parameters, transaction);
        //            result = feature with { Id = id };
        //        }
        //        else
        //        {
        //            var parameters = new
        //            {
        //                ts = TimeProvider.System.GetUtcNow().ToTimestampString(),
        //                repositoryId = feature.RepositoryId,
        //                parentId = feature.ParentId,
        //                name = feature.Name,
        //                shortDescription = feature.ShortDescription,
        //                description = feature.Description,
        //                isHidden = feature.IsHidden,
        //                isNew = feature.IsNew,
        //                hasImage = feature.HasImage,
        //                id = feature.Id
        //            };
        //            await db.ExecuteAsync(updateSql, parameters, transaction);
        //            result = feature;
        //        }

        //        var trx = Stopwatch.StartNew();
        //        await transaction.CommitAsync();
        //        trx.Stop();
        //        Logger.LogInformation(nameof(SaveFeature) + " | Transaction committed | ⏱️ {elapsed}", trx.Elapsed);
        //        return result;
    }

    public async Task CreateAsync(IEnumerable<TagGraph> tags, RepositoryId repositoryId)
        => _tagServices.Create(tags);

    public async Task<IEnumerable<Tag>> GetLatestTagsAsync(RepositoryId repositoryId)
        => _tagServices.GetLatestTags();

    public async Task<TagGraph> GetLatestTagAsync(RepositoryId repositoryId, bool preRelease)
        => _tagServices.GetLatestTag(preRelease);

    public async Task UpdateAsync(IEnumerable<Tag> tags)
        => _tagServices.Update(tags);

    public async Task<int?> GetFeatureId(RepositoryId repositoryId, string name)
        => _featureServices.GetId(name);
}