namespace IdeaBranch.Domain.Timeline;

/// <summary>
/// Represents a timeline event for SkiaSharp rendering with precision-aware temporal data.
/// </summary>
public sealed record TimelineEventView
{
    /// <summary>
    /// Gets the event ID.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the event title/description.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the event type/category for styling.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Gets the temporal range (start/end with precision).
    /// </summary>
    public TemporalRange When { get; init; }

    /// <summary>
    /// Gets optional tags for filtering.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Tags { get; init; }

    /// <summary>
    /// Gets the related node ID (if applicable).
    /// </summary>
    public Guid? NodeId { get; init; }

    /// <summary>
    /// Gets optional details text.
    /// </summary>
    public string? Details { get; init; }

    /// <summary>
    /// Gets the tags as a formatted string.
    /// </summary>
    public string TagsString
    {
        get
        {
            if (Tags == null || Tags.Count == 0)
                return "No tags";

            return string.Join(", ", Tags.Keys);
        }
    }

    /// <summary>
    /// Gets whether this is an annotation-related event.
    /// </summary>
    public bool IsAnnotationEvent => Type == "AnnotationCreated" || Type == "AnnotationUpdated";

    /// <summary>
    /// Initializes a new instance with the specified values.
    /// </summary>
    public TimelineEventView(
        string id,
        string title,
        string type,
        TemporalRange when,
        IReadOnlyDictionary<string, string>? tags = null,
        Guid? nodeId = null,
        string? details = null)
    {
        Id = id;
        Title = title;
        Type = type;
        When = when;
        Tags = tags;
        NodeId = nodeId;
        Details = details;
    }

    /// <summary>
    /// Converts a domain TimelineEvent to a TimelineEventView for rendering.
    /// </summary>
    public static TimelineEventView FromDomainEvent(Domain.TimelineEvent domainEvent)
    {
        var type = domainEvent.EventType switch
        {
            Domain.TimelineEventType.TopicCreated => "TopicCreated",
            Domain.TimelineEventType.TopicUpdated => "TopicUpdated",
            Domain.TimelineEventType.AnnotationCreated => "AnnotationCreated",
            Domain.TimelineEventType.AnnotationUpdated => "AnnotationUpdated",
            Domain.TimelineEventType.ConversationMessage => "ConversationMessage",
            _ => "Unknown"
        };

        var tags = domainEvent.TagIds.Count > 0
            ? domainEvent.TagIds.ToDictionary(id => id.ToString(), _ => "")
            : null;

        // Infer precision from timestamp (assume day precision for now)
        var when = TemporalRange.Point(domainEvent.Timestamp);

        return new TimelineEventView(
            domainEvent.Id.ToString(),
            domainEvent.Title,
            type,
            when,
            tags,
            domainEvent.NodeId,
            domainEvent.Details);
    }
}

