using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using CoordinatedTimeBot.Data.Abstractions;
using CoordinatedTimeBot.Host.Temporal;

namespace CoordinatedTimeBot.Host.Settings;

public class SettingsModule : ApplicationCommandModule<ApplicationCommandContext>
{
	private readonly IDataRepository _dataRepository;

	public SettingsModule(IDataRepository dataRepository)
	{
		_dataRepository = dataRepository;
	}

	[SlashCommand(
		name: "set-date-format",
		description: "Updates the date format preference for the current user.",
		Contexts = [InteractionContextType.Guild, InteractionContextType.BotDMChannel])]
	public async Task SetDateFormat(
		[SlashCommandParameter(Name = "format", Description = "The format to use.")] DateFormatKind format = DateFormatKind.YearMonthDay)
	{
		var id = Context?.User?.Id ?? 0;
		await _dataRepository.PutData(
			id,
			(SettingsData entity, DateFormatKind value) => entity with { Id = id, Format = value },
			format);

		var callback = InteractionCallback.Message(
			new InteractionMessageProperties()
				.WithContent($"Date format preference now set to {format}.")
				.WithFlags(MessageFlags.Ephemeral));

		await RespondAsync(callback);
	}

	[SlashCommand(
		name: "get-date-format",
		description: "Gets the date format preference for the current user.",
		Contexts = [InteractionContextType.Guild, InteractionContextType.BotDMChannel])]
	public async Task GetDateFormat()
	{
		var id = Context?.User?.Id ?? 0;
		var format = await _dataRepository.GetData(id, (SettingsData entity) => entity.Format);

		var callback = InteractionCallback.Message(
			new InteractionMessageProperties()
				.WithContent($"Date format preference is {format}.")
				.WithFlags(MessageFlags.Ephemeral));

		await RespondAsync(callback);
	}
}
