using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdeaBranch.Domain;

namespace IdeaBranch.Infrastructure.Analytics;

/// <summary>
/// Service for generating timeline data from temporal events.
/// </summary>
public class TimelineService : IAnalyticsService
{
    private readonly IConversationsRepository _conversationsRepository;
    private readonly IAnnotationsRepository _annotationsRepository;
    private readonly ITopicTreeRepository _topicTreeRepository;
    private readonly ITagTaxonomyRepository _tagTaxonomyRepository;

    /// <summary>
    /// Initializes a new instance with required repositories.
    /// </summary>
    public TimelineService(
        IConversationsRepository conversationsRepository,
        IAnnotationsRepository annotationsRepository,
        ITopicTreeRepository topicTreeRepository,
        ITagTaxonomyRepository tagTaxonomyRepository)
    {
        _conversationsRepository = conversationsRepository ?? throw new ArgumentNullException(nameof(conversationsRepository));
        _annotationsRepository = annotationsRepository ?? throw new ArgumentNullException(nameof(annotationsRepository));
        _topicTreeRepository = topicTreeRepository ?? throw new ArgumentNullException(nameof(topicTreeRepository));
        _tagTaxonomyRepository = tagTaxonomyRepository ?? throw new ArgumentNullException(nameof(tagTaxonomyRepository));
    }

    /// <inheritdoc/>
    public Task<WordCloudData> GenerateWordCloudAsync(
        WordCloudOptions options,
        CancellationToken cancellationToken = default)
    {
        // This is implemented by WordCloudService
        throw new NotImplementedException("Word cloud generation is handled by WordCloudService.");
    }

    /// <inheritdoc/>
    public async Task<TimelineData> GenerateTimelineAsync(
        TimelineOptions options,
        CancellationToken cancellationToken = default)
    {
        var events = new List<TimelineEvent>();

        // Collect events from different sources
        if (options.SourceTypes.Contains(EventSourceType.Topics))
        {
            var topicEvents = await CollectTopicEventsAsync(options, cancellationToken);
            events.AddRange(topicEvents);
        }

        if (options.SourceTypes.Contains(EventSourceType.Annotations))
        {
            var annotationEvents = await CollectAnnotationEventsAsync(options, cancellationToken);
            events.AddRange(annotationEvents);
        }

        if (options.SourceTypes.Contains(EventSourceType.Conversations))
        {
            var conversationEvents = await CollectConversationEventsAsync(options, cancellationToken);
            events.AddRange(conversationEvents);
        }

        // Sort chronologically
        events = events.OrderBy(e => e.Timestamp).ToList();

        // Group into bands based on grouping level
        var bands = GroupEventsIntoBands(events, options.Grouping);

        // Get tag IDs for events (if needed)
        foreach (var evt in events)
        {
            if (evt.NodeId.HasValue)
            {
                var tagIds = await _annotationsRepository.GetTagIdsAsync(evt.NodeId.Value, cancellationToken);
                evt.TagIds = tagIds;
            }
        }

        var earliestEvent = events.Count > 0 ? events[0].Timestamp : (DateTime?)null;
        var latestEvent = events.Count > 0 ? events[events.Count - 1].Timestamp : (DateTime?)null;

        return new TimelineData
        {
            Events = events.AsReadOnly(),
            Bands = bands.AsReadOnly(),
            Metadata = new TimelineMetadata
            {
                AppliedFilters = options,
                TotalEventCount = events.Count,
                EarliestEvent = earliestEvent,
                LatestEvent = latestEvent,
                GeneratedAt = DateTime.UtcNow
            }
        };
    }

    /// <summary>
    /// Collects topic events (created/updated).
    /// </summary>
    private async Task<List<TimelineEvent>> CollectTopicEventsAsync(
        TimelineOptions options,
        CancellationToken cancellationToken)
    {
        var events = new List<TimelineEvent>();
        var root = await _topicTreeRepository.GetRootAsync(cancellationToken);

        CollectTopicEventsFromNode(root, events, options);

        return events;
    }

    /// <summary>
    /// Recursively collects topic events from a node tree.
    /// </summary>
    private void CollectTopicEventsFromNode(TopicNode node, List<TimelineEvent> events, TimelineOptions options)
    {
        // Apply filters
        bool includeNode = true;

        if (options.NodeIds != null && options.NodeIds.Count > 0 && !options.NodeIds.Contains(node.Id))
            includeNode = false;

        if (options.StartDate.HasValue && node.CreatedAt < options.StartDate.Value && node.UpdatedAt < options.StartDate.Value)
            includeNode = false;

        if (options.EndDate.HasValue && node.CreatedAt > options.EndDate.Value && node.UpdatedAt > options.EndDate.Value)
            includeNode = false;

        if (includeNode)
        {
            // Created event
            if (!options.StartDate.HasValue || node.CreatedAt >= options.StartDate.Value)
            {
                if (!options.EndDate.HasValue || node.CreatedAt <= options.EndDate.Value)
                {
                    events.Add(new TimelineEvent
                    {
                        Id = Guid.NewGuid(),
                        Timestamp = node.CreatedAt,
                        EventType = TimelineEventType.TopicCreated,
                        Title = !string.IsNullOrWhiteSpace(node.Title) ? node.Title : "Topic Created",
                        Details = node.Prompt,
                        NodeId = node.Id
                    });
                }
            }

            // Updated event (if different from created)
            if (node.UpdatedAt > node.CreatedAt)
            {
                if (!options.StartDate.HasValue || node.UpdatedAt >= options.StartDate.Value)
                {
                    if (!options.EndDate.HasValue || node.UpdatedAt <= options.EndDate.Value)
                    {
                        events.Add(new TimelineEvent
                        {
                            Id = Guid.NewGuid(),
                            Timestamp = node.UpdatedAt,
                            EventType = TimelineEventType.TopicUpdated,
                            Title = !string.IsNullOrWhiteSpace(node.Title) ? node.Title : "Topic Updated",
                            Details = node.Response,
                            NodeId = node.Id
                        });
                    }
                }
            }
        }

        // Recurse into children
        foreach (var child in node.Children)
        {
            CollectTopicEventsFromNode(child, events, options);
        }
    }

    /// <summary>
    /// Collects annotation events.
    /// </summary>
    private async Task<List<TimelineEvent>> CollectAnnotationEventsAsync(
        TimelineOptions options,
        CancellationToken cancellationToken)
    {
        var events = new List<TimelineEvent>();

        // Get node IDs
        var nodeIds = new List<Guid>();

        if (options.NodeIds != null && options.NodeIds.Count > 0)
        {
            nodeIds.AddRange(options.NodeIds);
        }
        else
        {
            var root = await _topicTreeRepository.GetRootAsync(cancellationToken);
            nodeIds.AddRange(CollectNodeIds(root));
        }

        foreach (var nodeId in nodeIds)
        {
            IReadOnlyList<Annotation> annotations;

            if (options.TagIds != null && options.TagIds.Count > 0)
            {
                annotations = await _annotationsRepository.GetByNodeIdAndTagsAsync(
                    nodeId,
                    options.TagIds,
                    cancellationToken);
            }
            else
            {
                annotations = await _annotationsRepository.GetByNodeIdAsync(
                    nodeId,
                    cancellationToken);
            }

            foreach (var annotation in annotations)
            {
                // Apply date filter
                bool includeAnnotation = true;
                if (options.StartDate.HasValue && annotation.CreatedAt < options.StartDate.Value)
                    includeAnnotation = false;
                if (options.EndDate.HasValue && annotation.CreatedAt > options.EndDate.Value)
                    includeAnnotation = false;

                if (includeAnnotation)
                {
                    events.Add(new TimelineEvent
                    {
                        Id = Guid.NewGuid(),
                        Timestamp = annotation.CreatedAt,
                        EventType = TimelineEventType.AnnotationCreated,
                        Title = "Annotation Created",
                        Details = annotation.Comment ?? string.Empty,
                        NodeId = annotation.NodeId
                    });
                }
            }
        }

        return events;
    }

    /// <summary>
    /// Collects conversation events.
    /// </summary>
    private async Task<List<TimelineEvent>> CollectConversationEventsAsync(
        TimelineOptions options,
        CancellationToken cancellationToken)
    {
        var events = new List<TimelineEvent>();

        IReadOnlyList<ConversationMessage> messages;

        if (options.TagIds != null && options.TagIds.Count > 0)
        {
            messages = await _conversationsRepository.GetMessagesByTagsAsync(
                options.TagIds,
                options.IncludeTagDescendants,
                options.StartDate,
                options.EndDate,
                cancellationToken);
        }
        else if (options.NodeIds != null && options.NodeIds.Count > 0)
        {
            messages = await _conversationsRepository.GetMessagesByNodeIdsAsync(
                options.NodeIds,
                cancellationToken);

            // Apply date filter
            messages = messages
                .Where(m => !options.StartDate.HasValue || m.Timestamp >= options.StartDate.Value)
                .Where(m => !options.EndDate.HasValue || m.Timestamp <= options.EndDate.Value)
                .ToList()
                .AsReadOnly();
        }
        else
        {
            // Get all messages
            var root = await _topicTreeRepository.GetRootAsync(cancellationToken);
            var nodeIds = CollectNodeIds(root);
            messages = await _conversationsRepository.GetMessagesByNodeIdsAsync(nodeIds, cancellationToken);

            // Apply date filter
            messages = messages
                .Where(m => !options.StartDate.HasValue || m.Timestamp >= options.StartDate.Value)
                .Where(m => !options.EndDate.HasValue || m.Timestamp <= options.EndDate.Value)
                .ToList()
                .AsReadOnly();
        }

        foreach (var message in messages)
        {
            events.Add(new TimelineEvent
            {
                Id = message.Id,
                Timestamp = message.Timestamp,
                EventType = TimelineEventType.ConversationMessage,
                Title = message.MessageType == ConversationMessageType.Prompt ? "Prompt" : "Response",
                Details = message.Text,
                NodeId = message.NodeId
            });
        }

        return events;
    }

    /// <summary>
    /// Recursively collects all node IDs from a topic node tree.
    /// </summary>
    private List<Guid> CollectNodeIds(TopicNode node)
    {
        var nodeIds = new List<Guid> { node.Id };

        foreach (var child in node.Children)
        {
            nodeIds.AddRange(CollectNodeIds(child));
        }

        return nodeIds;
    }

    /// <summary>
    /// Groups events into bands based on grouping level.
    /// </summary>
    private List<TimelineBand> GroupEventsIntoBands(List<TimelineEvent> events, TimelineGrouping grouping)
    {
        if (events.Count == 0)
            return new List<TimelineBand>();

        var bands = new List<TimelineBand>();
        var grouped = grouping switch
        {
            TimelineGrouping.Day => events.GroupBy(e => e.Timestamp.Date),
            TimelineGrouping.Week => events.GroupBy(e => GetWeekStart(e.Timestamp)),
            TimelineGrouping.Month => events.GroupBy(e => new DateTime(e.Timestamp.Year, e.Timestamp.Month, 1)),
            _ => events.GroupBy(e => e.Timestamp.Date)
        };

        foreach (var group in grouped.OrderBy(g => g.Key))
        {
            var groupEvents = group.OrderBy(e => e.Timestamp).ToList();
            var startTime = group.Key;
            var endTime = grouping switch
            {
                TimelineGrouping.Day => startTime.AddDays(1),
                TimelineGrouping.Week => startTime.AddDays(7),
                TimelineGrouping.Month => startTime.AddMonths(1),
                _ => startTime.AddDays(1)
            };

            bands.Add(new TimelineBand
            {
                StartTime = startTime,
                EndTime = endTime,
                Events = groupEvents.AsReadOnly()
            });
        }

        return bands;
    }

    /// <summary>
    /// Gets the start of the week containing the given date.
    /// </summary>
    private static DateTime GetWeekStart(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff).Date;
    }
}

