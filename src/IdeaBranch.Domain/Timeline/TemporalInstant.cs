namespace IdeaBranch.Domain.Timeline;

/// <summary>
/// Represents a point in time with a specific precision level.
/// </summary>
public sealed record TemporalInstant
{
    /// <summary>
    /// Gets the date/time value.
    /// </summary>
    public DateTime Date { get; init; }

    /// <summary>
    /// Gets the precision level for this instant.
    /// </summary>
    public TemporalPrecision Precision { get; init; }

    /// <summary>
    /// Initializes a new instance with the specified date and precision.
    /// </summary>
    public TemporalInstant(DateTime date, TemporalPrecision precision)
    {
        Date = NormalizeDate(date, precision);
        Precision = precision;
    }

    /// <summary>
    /// Normalizes a date to match the specified precision level.
    /// </summary>
    private static DateTime NormalizeDate(DateTime date, TemporalPrecision precision)
    {
        return precision switch
        {
            TemporalPrecision.Year => new DateTime(date.Year, 1, 1),
            TemporalPrecision.Month => new DateTime(date.Year, date.Month, 1),
            TemporalPrecision.Day => new DateTime(date.Year, date.Month, date.Day),
            _ => date
        };
    }

    /// <summary>
    /// Converts a DateTime to a TemporalInstant with day precision.
    /// </summary>
    public static TemporalInstant FromDateTime(DateTime dateTime)
        => new(dateTime, TemporalPrecision.Day);

    /// <summary>
    /// Converts a DateTime to a TemporalInstant with the specified precision.
    /// </summary>
    public static TemporalInstant FromDateTime(DateTime dateTime, TemporalPrecision precision)
        => new(dateTime, precision);
}

