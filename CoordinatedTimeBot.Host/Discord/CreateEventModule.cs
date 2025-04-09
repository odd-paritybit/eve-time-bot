using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using CoordinatedTimeBot.Data.Abstractions;
using CoordinatedTimeBot.Host.Settings;
using CoordinatedTimeBot.Host.Temporal;

namespace CoordinatedTimeBot.Host.Discord;

public class CreateEventModule : ApplicationCommandModule<ApplicationCommandContext>
{
	private const string _createEventCommand = "create-event";
	private const string _createEventCliCommand = "create-event-cli";

	private readonly ILogger _logger;

	private readonly ITimeHelper _timeHelper;

	private readonly IDataRepository _repository;

	public CreateEventModule(ILogger<CreateEventModule> logger, ITimeHelper timeHelper, IDataRepository repository)
	{
		_logger = logger;
		_timeHelper = timeHelper;
		_repository = repository;
	}

	[SlashCommand(_createEventCommand, "Facilitates content pings using a data form.")]
	public async Task CreateEventWithForm()
	{
		_logger.LogInformation("{Command} executed by {User} with context {Guild} in channel {Channel}.",
			_createEventCommand, Context.User, Context.Guild, Context.Channel);
		var dateFormat = _timeHelper.GetParseTemplate((await _repository.GetEntity<SettingsData>(Context.User.Id)).Format);

		var callback = InteractionCallback.Modal(new ModalProperties(Identifiers.PingWithFormId, "Create Content Ping")
			.AddComponents(
				new TextInputProperties("What", TextInputStyle.Short, "What")
					.WithRequired(true)
					.WithPlaceholder("Brief description of the content you are pinging.")
					.WithValue(string.Empty),
				new TextInputProperties("Where", TextInputStyle.Short, "Form Location")
					.WithRequired(true)
					.WithPlaceholder("The space place for assembly.")
					.WithValue(string.Empty),
				new TextInputProperties("Date", TextInputStyle.Short, $"Date ({dateFormat.ToLower()})")
					.WithRequired(true)
					.WithValue(string.Empty)
					.WithPlaceholder(dateFormat.ToLower()),
				new TextInputProperties("Time", TextInputStyle.Short, "Time (24-hour input)")
					.WithRequired(true)
					.WithMinLength(5)
					.WithMaxLength(5)
					.WithPlaceholder("HH:MM")
					.WithValue($"{DateTime.UtcNow.AddHours(1).Hour:D2}:00"),
				new TextInputProperties("Details", TextInputStyle.Paragraph, "Additional Details")
					.WithRequired(false)
					.WithPlaceholder("Enter additional relevant details")
					.WithValue("")));

		await RespondAsync(callback);
	}

	[SlashCommand(
		name: _createEventCliCommand,
		description: "Pings content in the current channel.",
		Contexts = [InteractionContextType.Guild, InteractionContextType.DMChannel, InteractionContextType.BotDMChannel])]
	public async Task CreateEventWithCommand(
		[SlashCommandParameter(Name = "time", Description = "EVE Time in 24-hour format (HH:MM)")] string time,
		[SlashCommandParameter(Name = "date", Description = "EVE Date in your preferred format")] string date,
		[SlashCommandParameter(Name = "what", Description = "The content")] string what,
		[SlashCommandParameter(Name = "where", Description = "The form-up location")] string where = "",
		[SlashCommandParameter(Name = "details", Description = "Additional details")] string details = "",
		[SlashCommandParameter(Name = "output", Description = "How the content should be ouput")] CommandOutputKind output = CommandOutputKind.Channel,
		[SlashCommandParameter(Name = "notify", Description = "Who should be notified about the event [@everyone]")] Audience audience = Audience.Everyone)
	{
		_logger.LogInformation("{Command} executed by {User} with context {Guild} in channel {Channel}.",
			_createEventCliCommand, Context.User, Context.Guild, Context.Channel);
		var dateFormat = (await _repository.GetEntity<SettingsData>(Context.User.Id)).Format;
		var success = _timeHelper.TryParseDateAndTime(date, dateFormat, time, out var opTime);


		var message = new ContentMessage(what, opTime, where, details, audience);

		var callback = (success, output) switch
		{
			(true, CommandOutputKind.Channel) => InteractionCallback.Message(new InteractionMessageProperties()
					.WithContent(message.ToContentString())
					.WithAllowedMentions(AllowedMentionsProperties.All)),
			(true, CommandOutputKind.Ephemeral) => InteractionCallback.Message(new InteractionMessageProperties()
					.WithContent(message.ToContentString())
					.WithFlags(MessageFlags.Ephemeral)),
			(true, CommandOutputKind.CopyableEphemeral) => InteractionCallback.Message(new InteractionMessageProperties()
					.WithContent(message.ToCopyableString())
					.WithFlags(MessageFlags.Ephemeral)),
			(false, _) => InteractionCallback.Message(new InteractionMessageProperties()
					.WithContent($"""
						Something was wrong with the provided values.
						The date must be in your preferred format: `{_timeHelper.GetParseTemplate(dateFormat)}` (e.g. `{DateTime.UtcNow.ToString(_timeHelper.GetParseTemplate(dateFormat))}`)
						Time must be in 24-hour time (`hh:mm` from `00:00` to `23:59`)
						Hint: You can change your preferred date format with `/set-date-format`
						""")
					.WithFlags(MessageFlags.Ephemeral)),
			_ => throw new ArgumentOutOfRangeException(nameof(output), output, null)
		};

		await RespondAsync(callback);
	}
}
