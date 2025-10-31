using System;
using System.Globalization;

namespace IdeaBranch.App.Services;

/// <summary>
/// Service for locale-aware formatting of dates, times, and numbers.
/// </summary>
public static class LocalizationService
{
	/// <summary>
	/// Formats a date/time value using the current culture's format.
	/// Uses general date/time format (short date and short time).
	/// </summary>
	public static string FormatDateTime(DateTime dateTime)
	{
		return dateTime.ToString("g", CultureInfo.CurrentCulture);
	}

	/// <summary>
	/// Formats a date/time value using the current culture's format.
	/// Uses the specified format string.
	/// </summary>
	public static string FormatDateTime(DateTime dateTime, string format)
	{
		return dateTime.ToString(format, CultureInfo.CurrentCulture);
	}

	/// <summary>
	/// Formats a date value using the current culture's short date format.
	/// </summary>
	public static string FormatDate(DateTime date)
	{
		return date.ToString("d", CultureInfo.CurrentCulture);
	}

	/// <summary>
	/// Formats a time value using the current culture's short time format.
	/// </summary>
	public static string FormatTime(DateTime time)
	{
		return time.ToString("t", CultureInfo.CurrentCulture);
	}

	/// <summary>
	/// Formats a number using the current culture's number format.
	/// </summary>
	public static string FormatNumber(double number, string format = "N")
	{
		return number.ToString(format, CultureInfo.CurrentCulture);
	}

	/// <summary>
	/// Formats a number using the current culture's number format.
	/// </summary>
	public static string FormatNumber(int number, string format = "N0")
	{
		return number.ToString(format, CultureInfo.CurrentCulture);
	}

	/// <summary>
	/// Formats a currency value using the current culture's currency format.
	/// </summary>
	public static string FormatCurrency(decimal amount)
	{
		return amount.ToString("C", CultureInfo.CurrentCulture);
	}
}

