using CoordinatedTimeBot.Host.Discord;

namespace CoordinatedTimeBot.Host;

public record ContentMessage(string What, DateTimeOffset When, string Where, string Details, Audience Audience)
{
	public string ToContentString() => $"""
		## {What}
		> **EVE Time**: {When:dddd, MMMM d, yyyy HH:mm}
		> **Countdown: <t:{When.ToUnixTimeSeconds()}:R>**
		> **Localized Time**: <t:{When.ToUnixTimeSeconds()}:F>{(string.IsNullOrWhiteSpace(Where) ? string.Empty : $"{Environment.NewLine}> **Where**: {Where}")}{(string.IsNullOrWhiteSpace(Details) ? string.Empty : $"{Environment.NewLine}> **Details**: {Details.ReplaceLineEndings(Environment.NewLine + "> ")}")}
		{Audience switch { Audience.Everyone => "@everyone", Audience.Here => "@here", _ => string.Empty }}
		""";

	public string ToCopyableString() => $"""
		```
		{ToContentString()}
		```
		""";
}
