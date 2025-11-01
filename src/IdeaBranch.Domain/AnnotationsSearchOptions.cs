using System;
using System.Collections.Generic;

namespace IdeaBranch.Domain;

/// <summary>
/// Options for searching annotations with advanced filtering capabilities.
/// </summary>
public class AnnotationsSearchOptions
{
    /// <summary>
    /// Gets or sets tag IDs to include (AND logic - annotation must have all specified tags).
    /// </summary>
    public IReadOnlyList<Guid>? IncludeTags { get; set; }

    /// <summary>
    /// Gets or sets tag IDs to exclude (annotation must not have any of these tags).
    /// </summary>
    public IReadOnlyList<Guid>? ExcludeTags { get; set; }

    /// <summary>
    /// Gets or sets tag expression for complex tag filtering (e.g., "tag1 AND (tag2 OR tag3) BUT-NOT-IF (tag4 OR tag5)").
    /// This is a future enhancement; currently use IncludeTags/ExcludeTags.
    /// </summary>
    public string? TagExpression { get; set; }

    /// <summary>
    /// Gets or sets tag weight filters. Each filter specifies a tag ID, operator (gt, lt, between), and value(s).
    /// </summary>
    public IReadOnlyList<TagWeightFilter>? TagWeightFilters { get; set; }

    /// <summary>
    /// Gets or sets text to search for in comment field (LIKE %text%).
    /// </summary>
    public string? CommentContains { get; set; }

    /// <summary>
    /// Gets or sets the start of the UpdatedAt range filter (inclusive).
    /// </summary>
    public DateTime? UpdatedAtFrom { get; set; }

    /// <summary>
    /// Gets or sets the end of the UpdatedAt range filter (inclusive).
    /// </summary>
    public DateTime? UpdatedAtTo { get; set; }

    /// <summary>
    /// Gets or sets the start of the temporal/historical time range filter (inclusive).
    /// </summary>
    public DateTime? TemporalStart { get; set; }

    /// <summary>
    /// Gets or sets the end of the temporal/historical time range filter (inclusive).
    /// </summary>
    public DateTime? TemporalEnd { get; set; }

    /// <summary>
    /// Gets or sets the page size for pagination (default: all results).
    /// </summary>
    public int? PageSize { get; set; }

    /// <summary>
    /// Gets or sets the page token for pagination (offset/continuation token).
    /// </summary>
    public int? PageOffset { get; set; }
}

/// <summary>
/// Filter for tag weight range queries.
/// </summary>
public class TagWeightFilter
{
    /// <summary>
    /// Gets or sets the tag ID to filter by.
    /// </summary>
    public Guid TagId { get; set; }

    /// <summary>
    /// Gets or sets the operator: "gt" (greater than), "lt" (less than), "between" (inclusive range).
    /// </summary>
    public string Op { get; set; } = "gt";

    /// <summary>
    /// Gets or sets the first value (required for all operators).
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// Gets or sets the second value (required for "between" operator).
    /// </summary>
    public double? Value2 { get; set; }
}

