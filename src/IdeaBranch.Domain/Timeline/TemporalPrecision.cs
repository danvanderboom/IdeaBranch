namespace IdeaBranch.Domain.Timeline;

/// <summary>
/// Represents the temporal precision level for dates/times.
/// </summary>
public enum TemporalPrecision
{
    /// <summary>
    /// Year-level precision (e.g., 2024).
    /// </summary>
    Year,

    /// <summary>
    /// Month-level precision (e.g., 2024-03).
    /// </summary>
    Month,

    /// <summary>
    /// Day-level precision (e.g., 2024-03-15).
    /// </summary>
    Day
}

