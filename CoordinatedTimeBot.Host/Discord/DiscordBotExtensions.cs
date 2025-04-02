using NetCord;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;
using CoordinatedTimeBot.Data.Abstractions;
using CoordinatedTimeBot.Data.LiteDb;
using CoordinatedTimeBot.Host.Settings;
using CoordinatedTimeBot.Host.Temporal;

namespace CoordinatedTimeBot.Host.Discord;

public static class DiscordBotExtensions
{
	public static IServiceCollection AddDiscordBotServices(this IServiceCollection services)
	{
		services
			.AddTransient<ITimeHelper, TimeHelper>()
			.AddLiteDbServices()
			.AddDiscordGateway(options =>
			{
				// The token may be configured in the appsettings.json for debug purposes; this should
				// override the environment variable if it is set.
				options.Token = string.IsNullOrWhiteSpace(options.Token)
					? Environment.GetEnvironmentVariable("DISCORD_TOKEN")
					: options.Token;
			})
			.AddApplicationCommands()
			.AddComponentInteractions<ModalInteraction, ModalInteractionContext>()
			;

		return services;
	}

	public static IHost AddDiscordBotModules(this IHost host)
	{
		host
			.AddModules(typeof(CreateEventModule).Assembly)
			.UseGatewayEventHandlers();
		;

		return host;
	}

	public static async Task<IServiceProvider> InitializeDiscordBotServices(this IServiceProvider provider)
	{
		await provider
			.GetRequiredService<IDataRepository>()
			.Initialize<SettingsData>()
			;

		return provider;
	}
}
