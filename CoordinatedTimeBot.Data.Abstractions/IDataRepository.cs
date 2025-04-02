namespace CoordinatedTimeBot.Data.Abstractions;

public interface IDataRepository
{
	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	public Task Initialize<TEntity>()
		where TEntity : IScopedEntity, new();

	/// <summary>
	/// Gets scoped entity from the repository.
	/// </summary>
	/// <typeparam name="TEntity">Entity type</typeparam>
	/// <param name="id"></param>
	/// <returns></returns>
	Task<TEntity> GetEntity<TEntity>(ulong id)
		where TEntity : IScopedEntity, new();

	/// <summary>
	/// Gets scoped data from the repository.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	/// <param name="id"></param>
	/// <param name="Selector"></param>
	/// <returns></returns>
	Task<TValue> GetData<TEntity, TValue>(ulong id, Func<TEntity, TValue> Selector)
		where TEntity : IScopedEntity, new();

	/// <summary>
	/// Puts scoped entity into the repository.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <param name="entity"></param>
	/// <returns></returns>
	Task PutEntity<TEntity>(TEntity entity)
		where TEntity : IScopedEntity, new();

	/// <summary>
	/// Puts scoped data into the repository.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	/// <param name="id"></param>
	/// <param name="Updater"></param>
	/// <param name="value"></param>
	Task PutData<TEntity, TValue>(ulong id, Func<TEntity, TValue, TEntity> Updater, TValue value)
		where TEntity : IScopedEntity, new();
}
