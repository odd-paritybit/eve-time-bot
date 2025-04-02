using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;
using CoordinatedTimeBot.Data.Abstractions;
using CoordinatedTimeBot.Host.Settings;
using CoordinatedTimeBot.Host.Temporal;

namespace CoordinatedTimeBot.Host.Discord;

public class CreateEventModalModule : ComponentInteractionModule<ModalInteractionContext>
{
	private readonly ITimeHelper _timeHelper;

	private readonly IDataRepository _repository;

	public CreateEventModalModule(ITimeHelper timeHelper, IDataRepository repository)
	{
		_timeHelper = timeHelper;
		_repository = repository;
	}

	[ComponentInteraction(Identifiers.PingWithFormId)]
	public async Task CreateEventModalResponse()
	{
		var dateFormat = (await _repository.GetEntity<SettingsData>(Context.User.Id)).Format;

		var success = _timeHelper.TryParseDateAndTime(
			date: GetStringValue("Date"),
			dateFormat: dateFormat,
			time: GetStringValue("Time"),
			out var opDate);

		ContentMessage contentMessage = new(
			What: GetStringValue("What"),
			When: opDate,
			Where: GetStringValue("Where"),
			Details: GetStringValue("Details"),
			Audience: Audience.Everyone);

		var properties = success switch
		{
			true => new InteractionMessageProperties()
				.WithContent(contentMessage.ToContentString())
				.WithAllowedMentions(AllowedMentionsProperties.All),
			false => new InteractionMessageProperties()
				.WithContent($"""
					Something was wrong with the provided values.
					The date must be in your preferred format: `{_timeHelper.GetParseTemplate(dateFormat)}` (e.g. `{DateTime.UtcNow.ToString(_timeHelper.GetParseTemplate(dateFormat))}`)
					Time must be in 24-hour time (`hh:mm` from `00:00` to `23:59`)
					Hint: You can change your preferred date format with `/set-date-format`
					""")
				.WithFlags(MessageFlags.Ephemeral)
		};

		await Context.Interaction.SendResponseAsync(InteractionCallback.Message(properties));
	}

	private TValue GetValue<TValue>(string customId, Func<string, TValue> converter) =>
		converter(
			Context.Components
				.OfType<TextInput>()
				.Where(input => input.CustomId == customId)
				.Select(input => input.Value)
				.FirstOrDefault(string.Empty));

	private string GetStringValue(string customId) => GetValue(customId, x => x);
}
