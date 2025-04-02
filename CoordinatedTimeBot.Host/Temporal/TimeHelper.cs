using System.Globalization;

namespace CoordinatedTimeBot.Host.Temporal;

/// <summary>
/// Default implementation for parsing date and time strings.
/// </summary>
public class TimeHelper : ITimeHelper
{
	public string DayMonthYear { get; } = "dd/MM/yyyy";

	public string MonthDayYear { get; } = "MM/dd/yyyy";

	public string YearMonthDay { get; } = "yyyy/MM/dd";

	public string DefaultDateFormat { get; } = "yyyy/MM/dd";

	public string GetParseTemplate(DateFormatKind dateFormat) =>
		dateFormat switch
		{
			DateFormatKind.MonthDayYear => MonthDayYear,
			DateFormatKind.DayMonthYear => DayMonthYear,
			_ => YearMonthDay
		};

	public DateTimeOffset ParseDateAndTime(string date, DateFormatKind dateFormat, string time)
	{
		var proposedTime = time switch
		{
			"" => TimeOnly.ParseExact(DateTime.UtcNow.ToString("HH"), "HH", CultureInfo.InvariantCulture).AddHours(1),
			_ => TimeOnly.ParseExact(time, "HH:mm", CultureInfo.InvariantCulture)
		};

		var proposedDate = date switch
		{
			"" => DateOnly.FromDateTime(DateTime.UtcNow),
			_ => DateOnly.ParseExact(date, GetParseTemplate(dateFormat), CultureInfo.InvariantCulture)
		};

		var proposedDateTime = new DateTimeOffset(proposedDate, proposedTime, TimeSpan.Zero);

		return proposedDateTime < DateTimeOffset.UtcNow ? proposedDateTime.AddDays(1) : proposedDateTime;
	}

	public bool TryParseDateAndTime(string date, DateFormatKind dateFormat, string time, out DateTimeOffset result)
	{
		try
		{
			result = ParseDateAndTime(date, dateFormat, time);
			return true;
		}
		catch
		{
			result = default;
			return false;
		}
	}
}
