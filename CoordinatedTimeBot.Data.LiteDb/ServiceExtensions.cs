using Microsoft.Extensions.DependencyInjection;
using CoordinatedTimeBot.Data.Abstractions;

namespace CoordinatedTimeBot.Data.LiteDb;

public static class ServiceExtensions
{
	public static IServiceCollection AddLiteDbServices(this IServiceCollection collection)
	{
		return collection
			.AddTransient<IDataRepository, LiteDbDataRepository>()
			;
	}
}
