using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IdeaBranch.Domain;

/// <summary>
/// Service interface for analytics operations including word clouds and timelines.
/// </summary>
public interface IAnalyticsService
{
    /// <summary>
    /// Generates word cloud data from text sources.
    /// </summary>
    /// <param name="options">Word cloud generation options including source filters.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Word cloud data with word frequencies.</returns>
    Task<WordCloudData> GenerateWordCloudAsync(
        WordCloudOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates timeline data from temporal events.
    /// </summary>
    /// <param name="options">Timeline generation options including source filters.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Timeline data with chronological events.</returns>
    Task<TimelineData> GenerateTimelineAsync(
        TimelineOptions options,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Options for word cloud generation.
/// </summary>
public class WordCloudOptions
{
    /// <summary>
    /// Gets or sets the source types to include (prompts, responses, annotations, topics).
    /// </summary>
    public HashSet<TextSourceType> SourceTypes { get; set; } = new();

    /// <summary>
    /// Gets or sets tag IDs to filter by.
    /// </summary>
    public IReadOnlyList<Guid>? TagIds { get; set; }

    /// <summary>
    /// Gets or sets whether to include descendants of specified tags in hierarchical filtering.
    /// </summary>
    public bool IncludeTagDescendants { get; set; }

    /// <summary>
    /// Gets or sets the start date/time filter (optional).
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date/time filter (optional).
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Gets or sets the node IDs to filter by (optional). If null, includes all nodes.
    /// </summary>
    public IReadOnlyList<Guid>? NodeIds { get; set; }

    /// <summary>
    /// Gets or sets the minimum word frequency threshold (words with count below this are excluded).
    /// </summary>
    public int MinFrequency { get; set; } = 1;

    /// <summary>
    /// Gets or sets the maximum number of words to include (top N by frequency).
    /// </summary>
    public int? MaxWords { get; set; }
}

/// <summary>
/// Options for timeline generation.
/// </summary>
public class TimelineOptions
{
    /// <summary>
    /// Gets or sets the source types to include (topics, annotations, conversations).
    /// </summary>
    public HashSet<EventSourceType> SourceTypes { get; set; } = new();

    /// <summary>
    /// Gets or sets tag IDs to filter by.
    /// </summary>
    [Obsolete("Use TagSelections instead for per-tag descendant control.")]
    public IReadOnlyList<Guid>? TagIds { get; set; }

    /// <summary>
    /// Gets or sets whether to include descendants of specified tags in hierarchical filtering.
    /// </summary>
    [Obsolete("Use TagSelections instead for per-tag descendant control.")]
    public bool IncludeTagDescendants { get; set; }

    /// <summary>
    /// Gets or sets tag selections with per-tag descendant inclusion control.
    /// </summary>
    public IReadOnlyList<TagSelection>? TagSelections { get; set; }

    /// <summary>
    /// Gets or sets event types to filter by (Created/Updated mapping).
    /// If null or empty, all event types are included.
    /// </summary>
    public IReadOnlySet<TimelineEventType>? EventTypes { get; set; }

    /// <summary>
    /// Gets or sets the search query for free-text filtering (case-insensitive substring match).
    /// Minimum length is 2 characters. If null or empty, no search filtering is applied.
    /// </summary>
    public string? SearchQuery { get; set; }

    /// <summary>
    /// Gets or sets the start date/time filter (optional).
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date/time filter (optional).
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Gets or sets the node IDs to filter by (optional). If null, includes all nodes.
    /// </summary>
    public IReadOnlyList<Guid>? NodeIds { get; set; }

    /// <summary>
    /// Gets or sets the grouping level (day, week, month).
    /// </summary>
    public TimelineGrouping Grouping { get; set; } = TimelineGrouping.Day;
}

/// <summary>
/// Text source types for word cloud generation.
/// </summary>
public enum TextSourceType
{
    Prompts,
    Responses,
    Annotations,
    Topics
}

/// <summary>
/// Event source types for timeline generation.
/// </summary>
public enum EventSourceType
{
    Topics,
    Annotations,
    Conversations
}

/// <summary>
/// Timeline grouping levels.
/// </summary>
public enum TimelineGrouping
{
    Day,
    Week,
    Month
}

/// <summary>
/// Represents a tag selection with optional descendant inclusion.
/// </summary>
public sealed record TagSelection(Guid TagId, bool IncludeDescendants);

/// <summary>
/// Word cloud data containing word frequencies.
/// </summary>
public class WordCloudData
{
    /// <summary>
    /// Gets or sets the word frequencies sorted by frequency (descending).
    /// </summary>
    public IReadOnlyList<WordFrequency> WordFrequencies { get; set; } = Array.Empty<WordFrequency>();

    /// <summary>
    /// Gets or sets metadata about the generation.
    /// </summary>
    public WordCloudMetadata Metadata { get; set; } = new();
}

/// <summary>
/// Represents a word and its frequency.
/// </summary>
public class WordFrequency
{
    /// <summary>
    /// Gets or sets the word text.
    /// </summary>
    public string Word { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the frequency count.
    /// </summary>
    public int Frequency { get; set; }

    /// <summary>
    /// Gets or sets the normalized weight (0.0 to 1.0) for visualization.
    /// </summary>
    public double Weight { get; set; }
}

/// <summary>
/// Metadata about word cloud generation.
/// </summary>
public class WordCloudMetadata
{
    /// <summary>
    /// Gets or sets the applied filters.
    /// </summary>
    public WordCloudOptions AppliedFilters { get; set; } = new();

    /// <summary>
    /// Gets or sets the total word count across all sources.
    /// </summary>
    public int TotalWordCount { get; set; }

    /// <summary>
    /// Gets or sets the number of unique words.
    /// </summary>
    public int UniqueWordCount { get; set; }

    /// <summary>
    /// Gets or sets the generation timestamp.
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Timeline data containing chronological events.
/// </summary>
public class TimelineData
{
    /// <summary>
    /// Gets or sets the timeline events sorted chronologically.
    /// </summary>
    public IReadOnlyList<TimelineEvent> Events { get; set; } = Array.Empty<TimelineEvent>();

    /// <summary>
    /// Gets or sets the grouped timeline bands by time period.
    /// </summary>
    public IReadOnlyList<TimelineBand> Bands { get; set; } = Array.Empty<TimelineBand>();

    /// <summary>
    /// Gets or sets metadata about the generation.
    /// </summary>
    public TimelineMetadata Metadata { get; set; } = new();
}

/// <summary>
/// Represents a single timeline event.
/// </summary>
public class TimelineEvent
{
    /// <summary>
    /// Gets or sets the event ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the event timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the event type.
    /// </summary>
    public TimelineEventType EventType { get; set; }

    /// <summary>
    /// Gets or sets the event title/description.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional details.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Gets or sets the related node ID (if applicable).
    /// </summary>
    public Guid? NodeId { get; set; }

    /// <summary>
    /// Gets or sets tag IDs associated with this event.
    /// </summary>
    public IReadOnlyList<Guid> TagIds { get; set; } = Array.Empty<Guid>();
}

/// <summary>
/// Timeline event types.
/// </summary>
public enum TimelineEventType
{
    TopicCreated,
    TopicUpdated,
    AnnotationCreated,
    AnnotationUpdated,
    ConversationMessage
}

/// <summary>
/// Represents a timeline band (grouped events by time period).
/// </summary>
public class TimelineBand
{
    /// <summary>
    /// Gets or sets the start time of this band.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time of this band.
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Gets or sets the events in this band.
    /// </summary>
    public IReadOnlyList<TimelineEvent> Events { get; set; } = Array.Empty<TimelineEvent>();
}

/// <summary>
/// Metadata about timeline generation.
/// </summary>
public class TimelineMetadata
{
    /// <summary>
    /// Gets or sets the applied filters.
    /// </summary>
    public TimelineOptions AppliedFilters { get; set; } = new();

    /// <summary>
    /// Gets or sets the total event count.
    /// </summary>
    public int TotalEventCount { get; set; }

    /// <summary>
    /// Gets or sets the earliest event timestamp.
    /// </summary>
    public DateTime? EarliestEvent { get; set; }

    /// <summary>
    /// Gets or sets the latest event timestamp.
    /// </summary>
    public DateTime? LatestEvent { get; set; }

    /// <summary>
    /// Gets or sets the generation timestamp.
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

