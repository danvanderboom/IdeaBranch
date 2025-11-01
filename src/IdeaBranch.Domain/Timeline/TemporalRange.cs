namespace IdeaBranch.Domain.Timeline;

/// <summary>
/// Represents a temporal range with start and optional end, both with precision levels.
/// </summary>
public sealed record TemporalRange
{
    /// <summary>
    /// Gets the start instant.
    /// </summary>
    public TemporalInstant Start { get; init; }

    /// <summary>
    /// Gets the end instant, or null if this is a point event.
    /// </summary>
    public TemporalInstant? End { get; init; }

    /// <summary>
    /// Initializes a new instance with the specified start and optional end.
    /// </summary>
    public TemporalRange(TemporalInstant start, TemporalInstant? end = null)
    {
        if (end != null && end.Date < start.Date)
            throw new ArgumentException("End date must be after or equal to start date.", nameof(end));

        Start = start;
        End = end;
    }

    /// <summary>
    /// Creates a point-in-time range (start only).
    /// </summary>
    public static TemporalRange Point(TemporalInstant instant)
        => new(instant);

    /// <summary>
    /// Creates a point-in-time range from a DateTime with day precision.
    /// </summary>
    public static TemporalRange Point(DateTime dateTime)
        => new(TemporalInstant.FromDateTime(dateTime));

    /// <summary>
    /// Creates a duration range with start and end.
    /// </summary>
    public static TemporalRange Duration(TemporalInstant start, TemporalInstant end)
        => new(start, end);

    /// <summary>
    /// Creates a duration range from DateTime values with day precision.
    /// </summary>
    public static TemporalRange Duration(DateTime start, DateTime end)
        => new(TemporalInstant.FromDateTime(start), TemporalInstant.FromDateTime(end));
}

