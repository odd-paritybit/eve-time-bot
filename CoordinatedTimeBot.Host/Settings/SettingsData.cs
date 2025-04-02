using CoordinatedTimeBot.Data.Abstractions;
using CoordinatedTimeBot.Host.Temporal;

namespace CoordinatedTimeBot.Host.Settings;

public record SettingsData(ulong Id, string Scope, DateFormatKind Format) : IScopedEntity
{
	public SettingsData() : this(0, string.Empty, DateFormatKind.Undefined) { }
}
