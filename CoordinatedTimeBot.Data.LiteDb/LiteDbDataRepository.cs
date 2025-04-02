using LiteDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CoordinatedTimeBot.Data.Abstractions;

namespace CoordinatedTimeBot.Data.LiteDb;

public class LiteDbDataRepository : IDataRepository
{
	private readonly ILogger<LiteDbDataRepository> _logger;

	private readonly string _dataPath;

	public LiteDbDataRepository(ILogger<LiteDbDataRepository> logger, IConfiguration configuration)
	{
		_logger = logger;
		_dataPath = Path.Combine(
			Environment.CurrentDirectory,
			configuration["DataStorage:DbFileName"] ?? "Ctb.db");
	}

	public Task Initialize<TEntity>()
		where TEntity : IScopedEntity, new()
	{
		_logger.LogInformation("Initializing {EntityType} collection in data path {Path}", typeof(TEntity).Name, _dataPath);

		using var db = new LiteDatabase(_dataPath);
		var collection = db.GetCollection<TEntity>(typeof(TEntity).Name);
		collection.EnsureIndex(entity => entity.Id, true);

		return Task.CompletedTask;
	}

	public Task<TEntity> GetEntity<TEntity>(ulong id)
		where TEntity : IScopedEntity, new()
	{
		using var db = new LiteDatabase(_dataPath);
		var collection = db.GetCollection<TEntity>(typeof(TEntity).Name);
		TEntity entity = collection
			.Query()
			.Where(entity => entity.Id == id)
			.SingleOrDefault() ?? new() { Id = id };

		_logger.LogDebug("GetEntity<{EntityType}> found {Entity} with id {Id}", typeof(TEntity).Name, entity, id);
		return Task.FromResult(entity);
	}

	public Task<TValue> GetData<TEntity, TValue>(ulong id, Func<TEntity, TValue> Selector)
		where TEntity : IScopedEntity, new()
	{
		using var db = new LiteDatabase(_dataPath);
		var collection = db.GetCollection<TEntity>(typeof(TEntity).Name);

		var entity = collection
			.Query()
			.Where(entity => entity.Id == id)
			.SingleOrDefault() ?? new() { Id = id };

		_logger.LogDebug("GetData<{EntityType}, {ValueType}> found {Entity} in scope {Id}", typeof(TEntity).Name, typeof(TValue).Name, entity, id);
		var value = Selector(entity ?? new());

		_logger.LogDebug("Retrieved {EntityType} data with value {Value} from {Entity} for scope {Id}", typeof(TValue).Name, value, entity, id);
		return Task.FromResult(value);
	}

	public Task PutEntity<TEntity>(TEntity entity)
		where TEntity : IScopedEntity, new()
	{
		using var db = new LiteDatabase(_dataPath);
		var collection = db.GetCollection<TEntity>(typeof(TEntity).Name);
		collection.Upsert(entity);

		_logger.LogDebug("Upserted {EntityType} entity {Entity} for scope {Scope}", typeof(TEntity).Name, entity, entity.Id);
		return Task.CompletedTask;
	}

	public Task PutData<TEntity, TValue>(ulong id, Func<TEntity, TValue, TEntity> Updater, TValue value)
		where TEntity : IScopedEntity, new()
	{
		ArgumentNullException.ThrowIfNull(value, nameof(value));

		using var db = new LiteDatabase(_dataPath);
		var collection = db.GetCollection<TEntity>(typeof(TEntity).Name);
		var entity = collection
			.Query()
			.Where(entity => entity.Id == id)
			.SingleOrDefault() ?? new() { Id = id };

		_logger.LogDebug("GetData<{EntityType}, {ValueType}> found {Entity} in scope {Id}", typeof(TEntity).Name, typeof(TValue).Name, entity, id);

		var updatedEntity = entity switch
		{
			null => Updater(new TEntity { Id = id }, value),
			_ => Updater(entity, value)
		};

		collection.Upsert(updatedEntity);

		_logger.LogDebug("Updating {EntityType} data with value {Value} in {Entity} for scope {Id}", typeof(TValue).Name, value, updatedEntity, id);
		return Task.CompletedTask;
	}
}
