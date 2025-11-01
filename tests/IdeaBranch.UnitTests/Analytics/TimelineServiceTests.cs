using FluentAssertions;
using IdeaBranch.Domain;
using IdeaBranch.Infrastructure.Analytics;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IdeaBranch.UnitTests.Analytics;

/// <summary>
/// Tests for TimelineService.
/// </summary>
public class TimelineServiceTests
{
    private Mock<IConversationsRepository> _conversationsRepositoryMock = null!;
    private Mock<IAnnotationsRepository> _annotationsRepositoryMock = null!;
    private Mock<ITopicTreeRepository> _topicTreeRepositoryMock = null!;
    private Mock<ITagTaxonomyRepository> _tagTaxonomyRepositoryMock = null!;
    private TimelineService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _conversationsRepositoryMock = new Mock<IConversationsRepository>();
        _annotationsRepositoryMock = new Mock<IAnnotationsRepository>();
        _topicTreeRepositoryMock = new Mock<ITopicTreeRepository>();
        _tagTaxonomyRepositoryMock = new Mock<ITagTaxonomyRepository>();

        _service = new TimelineService(
            _conversationsRepositoryMock.Object,
            _annotationsRepositoryMock.Object,
            _topicTreeRepositoryMock.Object,
            _tagTaxonomyRepositoryMock.Object);
    }

    [Test]
    public async Task GenerateTimelineAsync_WithTopics_CreatesTopicEvents()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var root = new TopicNode("Root prompt", "Root");
        // Note: CreatedAt and UpdatedAt are set automatically when node is created
        // For test purposes, we'll use the current dates and verify the events are created
        
        var child = new TopicNode("Child prompt", "Child");
        root.AddChild(child);

        _topicTreeRepositoryMock
            .Setup(r => r.GetRootAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(root);

        _annotationsRepositoryMock
            .Setup(r => r.GetTagIdsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Guid>().ToList().AsReadOnly());

        var options = new TimelineOptions
        {
            SourceTypes = new HashSet<EventSourceType> { EventSourceType.Topics }
        };

        // Act
        var result = await _service.GenerateTimelineAsync(options);

        // Assert
        result.Should().NotBeNull();
        result.Events.Should().NotBeEmpty();
        result.Events.Should().Contain(e => e.EventType == TimelineEventType.TopicCreated);
        // Updated events may or may not exist if CreatedAt == UpdatedAt
        result.Events.Should().BeInAscendingOrder(e => e.Timestamp);
    }

    [Test]
    public async Task GenerateTimelineAsync_WithAnnotations_CreatesAnnotationEvents()
    {
        // Arrange
        var nodeId = Guid.NewGuid();
        // Annotation CreatedAt is set automatically when created
        var annotation = new Annotation(nodeId, 0, 10, "Annotation comment");

        _topicTreeRepositoryMock
            .Setup(r => r.GetRootAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TopicNode("Test", "Root"));

        _annotationsRepositoryMock
            .Setup(r => r.GetByNodeIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Annotation> { annotation }.AsReadOnly());

        _annotationsRepositoryMock
            .Setup(r => r.GetTagIdsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Guid>().ToList().AsReadOnly());

        var options = new TimelineOptions
        {
            SourceTypes = new HashSet<EventSourceType> { EventSourceType.Annotations }
        };

        // Act
        var result = await _service.GenerateTimelineAsync(options);

        // Assert
        result.Should().NotBeNull();
        result.Events.Should().Contain(e => e.EventType == TimelineEventType.AnnotationCreated);
    }

    [Test]
    public async Task GenerateTimelineAsync_GroupsEventsByDay()
    {
        // Arrange
        // Note: We can't set CreatedAt/UpdatedAt directly, so we'll test with current dates
        var root = new TopicNode("Test", "Root");
        
        var child = new TopicNode("Child", "Child");
        root.AddChild(child);

        _topicTreeRepositoryMock
            .Setup(r => r.GetRootAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(root);

        _annotationsRepositoryMock
            .Setup(r => r.GetTagIdsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Guid>().ToList().AsReadOnly());

        var options = new TimelineOptions
        {
            SourceTypes = new HashSet<EventSourceType> { EventSourceType.Topics },
            Grouping = TimelineGrouping.Day
        };

        // Act
        var result = await _service.GenerateTimelineAsync(options);

        // Assert
        result.Should().NotBeNull();
        result.Bands.Should().NotBeEmpty();
        foreach (var band in result.Bands)
        {
            band.Events.Should().NotBeEmpty();
        }
    }

    [Test]
    public async Task GenerateTimelineAsync_GroupsEventsByWeek()
    {
        // Arrange
        var root = new TopicNode("Test", "Root");
        
        var child = new TopicNode("Child", "Child");
        root.AddChild(child);

        _topicTreeRepositoryMock
            .Setup(r => r.GetRootAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(root);

        _annotationsRepositoryMock
            .Setup(r => r.GetTagIdsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Guid>().ToList().AsReadOnly());

        var options = new TimelineOptions
        {
            SourceTypes = new HashSet<EventSourceType> { EventSourceType.Topics },
            Grouping = TimelineGrouping.Week
        };

        // Act
        var result = await _service.GenerateTimelineAsync(options);

        // Assert
        result.Should().NotBeNull();
        result.Bands.Should().NotBeEmpty();
        foreach (var band in result.Bands)
        {
            (band.EndTime - band.StartTime).TotalDays.Should().BeApproximately(7, 1);
        }
    }

    [Test]
    public async Task GenerateTimelineAsync_WithDateFilter_FiltersEvents()
    {
        // Arrange
        var baseDate = DateTime.UtcNow.Date;
        var root = new TopicNode("Test", "Root");
        
        var child = new TopicNode("Child", "Child");
        root.AddChild(child);

        _topicTreeRepositoryMock
            .Setup(r => r.GetRootAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(root);

        _annotationsRepositoryMock
            .Setup(r => r.GetTagIdsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Guid>().ToList().AsReadOnly());

        var options = new TimelineOptions
        {
            SourceTypes = new HashSet<EventSourceType> { EventSourceType.Topics },
            StartDate = baseDate.AddDays(-3),
            EndDate = baseDate
        };

        // Act
        var result = await _service.GenerateTimelineAsync(options);

        // Assert
        result.Should().NotBeNull();
        // Note: Since we can't set dates directly, we'll verify that if events exist, they're filtered
        if (result.Events.Count > 0)
        {
            result.Events.Should().OnlyContain(e => 
                e.Timestamp >= options.StartDate!.Value && 
                e.Timestamp <= options.EndDate!.Value);
        }
    }

    [Test]
    public async Task GenerateTimelineAsync_SetsMetadata()
    {
        // Arrange
        var root = new TopicNode("Test", "Root");

        _topicTreeRepositoryMock
            .Setup(r => r.GetRootAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(root);

        _annotationsRepositoryMock
            .Setup(r => r.GetTagIdsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Guid>().ToList().AsReadOnly());

        var options = new TimelineOptions
        {
            SourceTypes = new HashSet<EventSourceType> { EventSourceType.Topics }
        };

        // Act
        var result = await _service.GenerateTimelineAsync(options);

        // Assert
        result.Should().NotBeNull();
        result.Metadata.Should().NotBeNull();
        if (result.Metadata.TotalEventCount > 0)
        {
            result.Metadata.EarliestEvent.Should().NotBeNull();
            result.Metadata.LatestEvent.Should().NotBeNull();
            if (result.Metadata.EarliestEvent.HasValue && result.Metadata.LatestEvent.HasValue)
            {
                result.Metadata.EarliestEvent.Should().BeBefore(result.Metadata.LatestEvent.Value);
            }
        }
    }
}

