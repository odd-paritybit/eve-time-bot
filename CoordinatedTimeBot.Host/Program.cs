using CoordinatedTimeBot.Host.Discord;

var builder = WebApplication.CreateBuilder(args);

builder.Services
	.AddDiscordBotServices()
	;

WebApplication host = builder.Build();

host
	.AddDiscordBotModules()
	;

host
	.MapGet("/health", () => "OK")
	;

await host.Services
	.InitializeDiscordBotServices()
	;

await host.RunAsync();
