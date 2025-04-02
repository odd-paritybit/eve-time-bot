using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using CoordinatedTimeBot.Data.Abstractions;
using CoordinatedTimeBot.Host.Settings;
using CoordinatedTimeBot.Host.Temporal;

namespace CoordinatedTimeBot.Host.Discord;

public class TimestampCommand : ApplicationCommandModule<ApplicationCommandContext>
{
	private const string _buildCoordinatedTimeCommand = "eve-time";

	private readonly ILogger _logger;

	private readonly ITimeHelper _timeHelper;

	private readonly IDataRepository _repository;

	public TimestampCommand(ILogger<CreateEventModule> logger, ITimeHelper timeHelper, IDataRepository repository)
	{
		_logger = logger;
		_timeHelper = timeHelper;
		_repository = repository;
	}

	[SlashCommand(
	name: _buildCoordinatedTimeCommand,
	description: "Converts the provided UTC [optional date and] time to a set of Discord timestamps.",
	Contexts = [InteractionContextType.Guild, InteractionContextType.DMChannel, InteractionContextType.BotDMChannel])]
	public async Task SplashEveTime(
		[SlashCommandParameter(Name = "time", Description = "Time in 24-hour format (hh:mm)")] string time,
		[SlashCommandParameter(Name = "date", Description = "Date in mm/dd format")] string date = "",
		[SlashCommandParameter(Name = "display", Description = "Where to display the output")] CommandOutputKind display = CommandOutputKind.Ephemeral)
	{
		_logger.LogInformation("{Command} executed by {User} with context {Guild} in channel {Channel}.",
			_buildCoordinatedTimeCommand, Context.User, Context.Guild, Context.Channel);

		var dateFormat = (await _repository.GetEntity<SettingsData>(Context.User.Id)).Format;
		var success = _timeHelper.TryParseDateAndTime(date, dateFormat, time, out var opTime);

		var callback = (success, display) switch
		{
			(true, CommandOutputKind.Ephemeral) => InteractionCallback.Message(
				new InteractionMessageProperties()
					.WithContent(GetDisplayableTime(opTime))
					.WithFlags(MessageFlags.Ephemeral)),
			(true, CommandOutputKind.Channel) => InteractionCallback.Message(
				new InteractionMessageProperties()
					.WithContent(GetDisplayableTime(opTime))),
			(true, CommandOutputKind.CopyableEphemeral) => InteractionCallback.Message(
				new InteractionMessageProperties()
					.WithContent(GetCopyableTime(opTime))
					.WithFlags(MessageFlags.Ephemeral)),
			_ => InteractionCallback.Message(
				new InteractionMessageProperties()
					.WithContent($"""
						Something was wrong with the provided values.
						The date must be in your preferred format: `{_timeHelper.GetParseTemplate(dateFormat)}` (e.g. `{DateTime.UtcNow.ToString(_timeHelper.GetParseTemplate(dateFormat))}`)
						Time must be in 24-hour time (e.g. `hh:mm` from `00:00` to `23:59`)
						Hint: You can change your preferred date format with `/set-date-format`
						""")
					.WithFlags(MessageFlags.Ephemeral))
		};

		await RespondAsync(callback);
	}

	private static string GetDisplayableTime(DateTimeOffset opTime) =>
		$"""
		EVE Time: {opTime:F}
		Local: <t:{opTime.ToUnixTimeSeconds()}:F>
		Countdown: <t:{opTime.ToUnixTimeSeconds()}:R>
		""";

	private static string GetCopyableTime(DateTimeOffset opTime) =>
		$"""
		```
		{GetDisplayableTime(opTime)}
		```
		""";
}
