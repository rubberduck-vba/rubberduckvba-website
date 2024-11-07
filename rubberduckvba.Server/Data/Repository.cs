using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using rubberduckvba.Server.Model.Entity;
using System.Data;

namespace rubberduckvba.Server.Data;

public interface IRepository<TEntity> where TEntity : Entity
{
    int GetId(string name);
    TEntity GetById(int id);
    IEnumerable<TEntity> GetAll(int? parentId = default);
    TEntity Insert(TEntity entity);
    IEnumerable<TEntity> Insert(IEnumerable<TEntity> entities);
    void Update(TEntity entity);
    void Update(IEnumerable<TEntity> entities);
}

public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : Entity
{
    private readonly string _connectionString;

    protected Repository(IOptions<ConnectionSettings> settings)
    {
        _connectionString = settings.Value.RubberduckDb ?? throw new InvalidOperationException();
    }

    protected IEnumerable<T> Query<T>(Func<IDbConnection, IEnumerable<T>> query)
    {
        using var db = new SqlConnection(_connectionString);
        db.Open();
        return query(db);
    }

    protected T Get<T>(Func<IDbConnection, T> query)
    {
        using var db = new SqlConnection(_connectionString);
        db.Open();
        return query(db);
    }

    protected void Execute(Action<IDbConnection> action)
    {
        using var db = new SqlConnection(_connectionString);
        db.Open();
        action(db);
    }

    protected abstract string TableName { get; }
    protected abstract string SelectSql { get; }
    protected abstract string InsertSql { get; }
    protected abstract string UpdateSql { get; }

    protected virtual string? ParentFKColumnName => "ParentFKColumnName";

    public virtual int GetId(string name) => Get(db => db.QuerySingle<int>($"SELECT [Id] FROM [dbo].[{TableName}] WHERE [Name]=@name", new { name }));
    public virtual TEntity GetById(int id) => Get(db => db.QuerySingle<TEntity>(SelectSql + " WHERE [Id]=@id", new { id }));
    public virtual IEnumerable<TEntity> GetAll(int? parentId = default) =>
        ParentFKColumnName is null || !parentId.HasValue
            ? Query(db => db.Query<TEntity>(SelectSql))
            : Query(db => db.Query<TEntity>($"{SelectSql} WHERE [{ParentFKColumnName}]=@parentId", new { parentId }));
    public virtual TEntity Insert(TEntity entity) => Insert([entity]).Single();
    public virtual IEnumerable<TEntity> Insert(IEnumerable<TEntity> entities)
    {
        using var db = new SqlConnection(_connectionString);
        db.Open();

        using var transaction = db.BeginTransaction();
        var inserts = new List<TEntity>();
        foreach (var entity in entities)
        {
            var id = db.ExecuteScalar<int>(InsertSql + "; SELECT SCOPE_IDENTITY()", entity, transaction);
            inserts.Add(entity with { Id = id });
        }

        transaction.Commit();
        return inserts;
    }

    public virtual void Update(TEntity entity) => Update([entity]);
    public virtual void Update(IEnumerable<TEntity> entities)
    {
        using var db = new SqlConnection(_connectionString);
        db.Open();

        using var transaction = db.BeginTransaction();
        foreach (var entity in entities)
        {
            db.Execute(UpdateSql, entity, transaction);
        }

        transaction.Commit();
    }
}
