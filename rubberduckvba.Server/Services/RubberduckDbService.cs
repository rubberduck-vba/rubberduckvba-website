using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using rubberduckvba.Server.ContentSynchronization.Pipeline.Sections.Context;
using rubberduckvba.Server.Data;
using rubberduckvba.Server.Model;
using rubberduckvba.Server.Model.Entity;
using rubberduckvba.Server.Services.rubberduckdb;
using System.Security.Principal;
using System.Text.Json;
using static Dapper.SqlMapper;

namespace rubberduckvba.Server.Services;

public interface IStagingServices
{
    Task StageAsync(StagingContext staging, CancellationToken token);
}

public class StagingServices(TagServices tagService, FeatureServices featureServices) : IStagingServices
{
    public async Task StageAsync(StagingContext context, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var updatedTags = context.Tags.Where(e => e.Id != default);
        tagService.Update(updatedTags);

        token.ThrowIfCancellationRequested();
        var newTags = context.Tags.Where(e => e.Id == default);
        tagService.Create(newTags);

        token.ThrowIfCancellationRequested();
        var updatedInspections = context.Inspections.Where(e => e.Id != default);
        featureServices.Update(updatedInspections);

        token.ThrowIfCancellationRequested();
        var newInspections = context.Inspections.Where(e => e.Id == default);
        featureServices.Insert(newInspections);

        token.ThrowIfCancellationRequested();
        var updatedQuickFixes = context.QuickFixes.Where(e => e.Id != default);
        featureServices.Update(updatedQuickFixes);

        token.ThrowIfCancellationRequested();
        var newQuickFixes = context.QuickFixes.Where(e => e.Id == default);
        featureServices.Insert(newQuickFixes);

        token.ThrowIfCancellationRequested();
        var updatedAnnotations = context.Annotations.Where(e => e.Id != default);
        featureServices.Update(updatedAnnotations);

        token.ThrowIfCancellationRequested();
        var newAnnotations = context.Annotations.Where(e => e.Id == default);
        featureServices.Insert(newAnnotations);
    }
}

public enum RepositoryId
{
    Rubberduck = 1,
    Rubberduck3 = 2
}

public interface IAuditService
{
    Task CreateFeature(Feature feature, IIdentity identity);
    Task DeleteFeature(Feature feature, IIdentity identity);
    Task UpdateFeature(Feature feature, IIdentity identity);


    Task<IEnumerable<T>> GetPendingItems<T>(int? featureId = default) where T : AuditEntity;

    Task Approve<T>(T entity, IIdentity identity) where T : AuditEntity;
    Task Reject<T>(T entity, IIdentity identity) where T : AuditEntity;
}

public class AuditService : IAuditService
{
    private readonly string _connectionString;
    private readonly ILogger _logger;

    public AuditService(IOptions<ConnectionSettings> settings, ILogger<ServiceLogger> logger)
    {
        _connectionString = settings.Value.RubberduckDb ?? throw new InvalidOperationException("ConnectionString 'RubberduckDb' could not be retrieved.");
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private async Task<SqlConnection> GetDbConnection()
    {
        var db = new SqlConnection(_connectionString);
        await db.OpenAsync();

        return db;
    }

    public async Task Approve<T>(T entity, IIdentity identity) where T : AuditEntity
    {
        var procName = entity switch
        {
            FeatureOpEntity => "audits.ApproveFeatureOp",
            FeatureEditEntity => "audits.ApproveFeatureEdit",
            _ => throw new NotSupportedException($"The entity type {typeof(T).Name} is not supported for approval."),
        };
        await ApproveOrReject(procName, entity.Id, identity);
    }

    public async Task Reject<T>(T entity, IIdentity identity) where T : AuditEntity
    {
        var procName = entity switch
        {
            FeatureOpEntity => "audits.RejectFeatureOp",
            FeatureEditEntity => "audits.RejectFeatureEdit",
            _ => throw new NotSupportedException($"The entity type {typeof(T).Name} is not supported for approval."),
        };
        await ApproveOrReject(procName, entity.Id, identity);
    }

    private async Task ApproveOrReject(string procedure, int id, IIdentity identity)
    {
        var login = identity?.Name ?? throw new ArgumentNullException(nameof(identity), "Identity name cannot be null.");

        using var db = await GetDbConnection();
        db.Execute($"EXEC {procedure} @id, @login", new { id, login });
    }

    public async Task CreateFeature(Feature feature, IIdentity identity)
    {
        await SubmitFeatureOp(feature, identity, FeatureOperation.Create);
    }

    public async Task DeleteFeature(Feature feature, IIdentity identity)
    {
        await SubmitFeatureOp(feature, identity, FeatureOperation.Delete);
    }

    public async Task UpdateFeature(Feature feature, IIdentity identity)
    {
        await SubmitFeatureEdit(feature, identity);
    }

    private async Task SubmitFeatureOp(Feature feature, IIdentity identity, FeatureOperation operation)
    {
        var login = identity?.Name ?? throw new ArgumentNullException(nameof(identity), "Identity name cannot be null.");
        const string sql = $@"INSERT INTO audits.FeatureOps (DateInserted,Author,FeatureName,FeatureAction,ParentId,Title,ShortDescription,Description,IsNew,IsHidden,HasImage,Links)
                             VALUES (GETDATE(),@login,@name,@action,@parentId,@title,@summary,@description,@isNew,@isHidden,@hasImage,@links);";

        using var db = await GetDbConnection();
        await db.ExecuteAsync(sql, new
        {
            login,
            name = feature.Name,
            action = Convert.ToInt32(operation),
            parentId = feature.FeatureId,
            title = feature.Title,
            summary = feature.ShortDescription,
            description = feature.Description,
            isNew = feature.IsNew,
            isHidden = feature.IsHidden,
            hasImage = feature.HasImage,
            links = JsonSerializer.Serialize(feature.Links)
        });
    }

    private async Task SubmitFeatureEdit(Feature feature, IIdentity identity)
    {
        var login = identity?.Name ?? throw new ArgumentNullException(nameof(identity), "Identity name cannot be null.");
        const string sql = $@"INSERT INTO audits.FeatureEdits (DateInserted,Author,FeatureId,FieldName,ValueBefore,ValueAfter)
                              VALUES (GETDATE(),@login,@featureId,@fieldName,@valueBefore,@valueAfter);";

        using var db = await GetDbConnection();

        var current = await db.QuerySingleOrDefaultAsync<FeatureEntity>("SELECT * FROM dbo.Features WHERE Id = @featureId", new { featureId = feature.Id });
        var editableFields = await db.QueryAsync<string>("SELECT FieldName FROM audits.v_FeatureColumns");

        string? fieldName = null;
        string? valueBefore = null;
        string? valueAfter = null;

        foreach (var name in editableFields)
        {
            var currentProperty = current.GetType().GetProperty(name);
            var property = feature.GetType().GetProperty(name)!;
            var asJson = property.PropertyType.IsClass && property.PropertyType != typeof(string);

            valueBefore = asJson ? JsonSerializer.Serialize(currentProperty?.GetValue(current)) : currentProperty?.GetValue(current)?.ToString() ?? string.Empty;
            valueAfter = asJson ? JsonSerializer.Serialize(property?.GetValue(feature)) : property?.GetValue(feature)?.ToString() ?? string.Empty;

            if (valueBefore != valueAfter)
            {
                fieldName = name;
                break;
            }
        }

        if (fieldName is null)
        {
            _logger.LogInformation("No change detected for field in feature '{FeatureName}'. No audit entry created.", feature.Name);
            return;
        }

        await db.ExecuteAsync(sql, new
        {
            login,
            featureId = feature.Id,
            fieldName,
            valueBefore,
            valueAfter
        });
    }

    public async Task<IEnumerable<T>> GetPendingItems<T>(int? featureId = default) where T : AuditEntity
    {
        using var db = await GetDbConnection();
        var (tableName, columns) = typeof(T).Name switch
        {
            nameof(FeatureOpEntity) => ("audits.FeatureOps src", string.Join(',', typeof(FeatureOpEntity).GetProperties().Where(p => p.CanWrite).Select(p => $"src.[{p.Name}]"))),
            nameof(FeatureEditEntity) => ("audits.FeatureEdits src", string.Join(',', typeof(FeatureEditEntity).GetProperties().Where(p => p.CanWrite).Select(p => $"src.[{p.Name}]"))),
            _ => throw new NotSupportedException($"The entity type {typeof(T).Name} is not supported for pending items retrieval."),
        };

        const string pendingFilter = "src.[ApprovedBy] IS NULL AND src.[RejectedBy] IS NULL";

        var sql = featureId.HasValue
            ? typeof(T).Name switch
            {
                nameof(FeatureOpEntity) => $"SELECT {columns} FROM {tableName} INNER JOIN dbo.Features f ON src.[FeatureName] = f.[Name] WHERE {pendingFilter} AND f.[Id] = {featureId}",
                nameof(FeatureEditEntity) => $"SELECT {columns} FROM {tableName} WHERE {pendingFilter} AND src.[FeatureId] = {featureId}",
                _ => throw new NotSupportedException($"The entity type {typeof(T).Name} is not supported for pending items retrieval."),
            }
            : $"SELECT {columns} FROM {tableName} WHERE {pendingFilter}";

        sql += " ORDER BY src.[DateInserted] DESC";
        return await db.QueryAsync<T>(sql);
    }
}

public interface IRubberduckDbService
{
    Task<IEnumerable<HangfireJobState>> GetJobStateAsync();

    Task<IEnumerable<Tag>> GetLatestTagsAsync(RepositoryId repositoryId);
    Task<TagGraph> GetLatestTagAsync(RepositoryId repositoryId, bool includePreRelease);
    Task UpdateAsync(IEnumerable<Tag> tags);
    Task CreateAsync(IEnumerable<TagGraph> tags, RepositoryId repositoryId);

    Task<IEnumerable<Feature>> GetTopLevelFeatures(RepositoryId? repositoryId = default);
    Task<FeatureGraph> ResolveFeature(RepositoryId repositoryId, string name);
    Task<int?> GetFeatureId(RepositoryId repositoryId, string name);
}

public class RubberduckDbService : IRubberduckDbService
{
    private readonly TagServices _tagServices;
    private readonly FeatureServices _featureServices;
    private readonly HangfireJobStateRepository _hangfireJobState;

    public RubberduckDbService(TagServices tagServices, FeatureServices featureServices, HangfireJobStateRepository hangfireJobState)
    {
        _tagServices = tagServices;
        _featureServices = featureServices;
        _hangfireJobState = hangfireJobState;
    }

    public async Task<IEnumerable<Tag>> GetAllTagsAsync()
    {
        return _tagServices.GetAllTags();
    }

    public async Task<IEnumerable<Feature>> GetTopLevelFeatures(RepositoryId? repositoryId = default)
    {
        return _featureServices.Get(topLevelOnly: true);
    }

    public async Task<FeatureGraph> ResolveFeature(RepositoryId repositoryId, string name)
    {
        var features = _featureServices.Get(topLevelOnly: false).ToList();
        var feature = features.Single(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));
        var children = features.Where(e => e.FeatureId == feature.Id);
        return new FeatureGraph(feature.ToEntity())
        {
            Features = children.ToArray()
        };
    }

    public async Task CreateAsync(IEnumerable<TagGraph> tags, RepositoryId repositoryId)
        => await Task.Run(() => _tagServices.Create(tags));

    public async Task<IEnumerable<Tag>> GetLatestTagsAsync(RepositoryId repositoryId)
        => await Task.Run(() => _tagServices.GetLatestTags());

    public async Task<TagGraph> GetLatestTagAsync(RepositoryId repositoryId, bool preRelease)
        => await Task.Run(() => _tagServices.GetLatestTag(preRelease));

    public async Task UpdateAsync(IEnumerable<Tag> tags)
        => await Task.Run(() => _tagServices.Update(tags));

    public async Task<int?> GetFeatureId(RepositoryId repositoryId, string name)
        => await Task.Run(() => _featureServices.GetId(name));

    public async Task<IEnumerable<HangfireJobState>> GetJobStateAsync()
        => await Task.Run(() => _hangfireJobState.GetAll());
}
