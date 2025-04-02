namespace CoordinatedTimeBot.Host.Temporal;

public interface ITimeHelper
{
	/// <summary>
	/// Formatting string for DayMonthYear date format.
	/// </summary>
	string DayMonthYear { get; }

	/// <summary>
	/// Formatting string for MonthDayYear date format.
	/// </summary>
	string MonthDayYear { get; }

	/// <summary>
	/// Formatting string for YearMonthDay date format.
	/// </summary>
	string YearMonthDay { get; }

	/// <summary>
	/// The default date formatting string.
	/// </summary>
	string DefaultDateFormat { get; }

	/// <summary>
	/// Parses a date and time string into a DateTimeOffset.
	/// </summary>
	/// <param name="date">The date</param>
	/// <param name="dateFormat">The format to use when parsing</param>
	/// <param name="time">The time</param>
	/// <returns>The parsed date and time</returns>
	DateTimeOffset ParseDateAndTime(string date, DateFormatKind dateFormat, string time);

	/// <summary>
	/// Tries to parse a date and time string into a DateTimeOffset.
	/// </summary>
	/// <param name="date">The date</param>
	/// <param name="dateFormat">The format to use when parsing</param>
	/// <param name="time">The time</param>
	/// <param name="result">The date and time if it could be parsed.</param>
	/// <returns>True if successful, false otherwise</returns>
	bool TryParseDateAndTime(string date, DateFormatKind dateFormat, string time, out DateTimeOffset result);

	/// <summary>
	/// Gets the formatting string from the enumeration.
	/// </summary>
	/// <param name="dateFormat">The kind of format expected</param>
	/// <returns>A string pattern to use for parsing dates</returns>
	string GetParseTemplate(DateFormatKind dateFormat);
}