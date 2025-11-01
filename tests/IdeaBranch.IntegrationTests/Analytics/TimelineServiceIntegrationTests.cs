using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using IdeaBranch.Domain;
using IdeaBranch.Infrastructure.Analytics;
using IdeaBranch.Infrastructure.Storage;
using IdeaBranch.IntegrationTests.Storage;
using NUnit.Framework;

namespace IdeaBranch.IntegrationTests.Analytics;

/// <summary>
/// Integration tests for TimelineService with real database repositories.
/// </summary>
[TestFixture]
public class TimelineServiceIntegrationTests
{
    private SqliteTestDatabase _testDb = null!;
    private TimelineService _service = null!;
    private IConversationsRepository _conversationsRepository = null!;
    private IAnnotationsRepository _annotationsRepository = null!;
    private ITopicTreeRepository _topicTreeRepository = null!;
    private ITagTaxonomyRepository _tagTaxonomyRepository = null!;
    private TopicDb _topicDb = null!;

    [SetUp]
    public void SetUp()
    {
        _testDb = new SqliteTestDatabase();
        _topicDb = new TopicDb($"Data Source={_testDb.DbPath}");
        
        _annotationsRepository = new SqliteAnnotationsRepository(_topicDb.Connection);
        _tagTaxonomyRepository = new SqliteTagTaxonomyRepository(_topicDb.Connection);
        _topicTreeRepository = new SqliteTopicTreeRepository(_topicDb, null);
        
        _conversationsRepository = new SqliteConversationsRepository(
            _topicDb.Connection,
            _annotationsRepository,
            _tagTaxonomyRepository);
        
        _service = new TimelineService(
            _conversationsRepository,
            _annotationsRepository,
            _topicTreeRepository,
            _tagTaxonomyRepository);
    }

    [TearDown]
    public void TearDown()
    {
        (_topicTreeRepository as IDisposable)?.Dispose();
        _topicDb?.Dispose();
        _testDb?.Dispose();
    }

    [Test]
    public async Task GenerateTimelineAsync_WithTopics_CreatesTopicEvents()
    {
        // Arrange
        var root = await _topicTreeRepository.GetRootAsync();
        root.Prompt = "Test question";
        root.SetResponse("Test response");
        
        var child = new TopicNode("Child question", "Child");
        child.SetResponse("Child response");
        root.AddChild(child);
        await _topicTreeRepository.SaveAsync(root);

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
        result.Events.Should().BeInAscendingOrder(e => e.Timestamp);
    }

    [Test]
    public async Task GenerateTimelineAsync_WithAnnotations_CreatesAnnotationEvents()
    {
        // Arrange
        var root = await _topicTreeRepository.GetRootAsync();
        var nodeId = root.Id;

        var annotation = new Annotation(nodeId, 0, 10, "Test annotation comment");
        await _annotationsRepository.SaveAsync(annotation);

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
        var root = await _topicTreeRepository.GetRootAsync();
        
        var child1 = new TopicNode("Child 1", "Child1");
        var child2 = new TopicNode("Child 2", "Child2");
        root.AddChild(child1);
        root.AddChild(child2);
        await _topicTreeRepository.SaveAsync(root);

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
        result.Bands.Should().OnlyContain(b => b.Events.Any());
        
        foreach (var band in result.Bands)
        {
            var daysDifference = (band.EndTime - band.StartTime).TotalDays;
            daysDifference.Should().BeLessThan(2.0); // Should be approximately 1 day
        }
    }

    [Test]
    public async Task GenerateTimelineAsync_GroupsEventsByWeek()
    {
        // Arrange
        var root = await _topicTreeRepository.GetRootAsync();
        
        var child = new TopicNode("Child", "Child");
        root.AddChild(child);
        await _topicTreeRepository.SaveAsync(root);

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
    public async Task GenerateTimelineAsync_GroupsEventsByMonth()
    {
        // Arrange
        var root = await _topicTreeRepository.GetRootAsync();
        
        var child = new TopicNode("Child", "Child");
        root.AddChild(child);
        await _topicTreeRepository.SaveAsync(root);

        var options = new TimelineOptions
        {
            SourceTypes = new HashSet<EventSourceType> { EventSourceType.Topics },
            Grouping = TimelineGrouping.Month
        };

        // Act
        var result = await _service.GenerateTimelineAsync(options);

        // Assert
        result.Should().NotBeNull();
        result.Bands.Should().NotBeEmpty();
        foreach (var band in result.Bands)
        {
            var daysDifference = (band.EndTime - band.StartTime).TotalDays;
            daysDifference.Should().BeGreaterThanOrEqualTo(28); // At least 28 days (month)
            daysDifference.Should().BeLessThanOrEqualTo(31); // At most 31 days
        }
    }

    [Test]
    public async Task GenerateTimelineAsync_SetsMetadata()
    {
        // Arrange
        var root = await _topicTreeRepository.GetRootAsync();
        root.SetResponse("Test");
        await _topicTreeRepository.SaveAsync(root);

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
        }
    }

    [Test]
    public async Task GenerateTimelineAsync_WithMultipleSourceTypes_AggregatesEvents()
    {
        // Arrange
        var root = await _topicTreeRepository.GetRootAsync();
        root.SetResponse("Test response");
        
        var child = new TopicNode("Child", "Child");
        root.AddChild(child);
        await _topicTreeRepository.SaveAsync(root);

        var annotation = new Annotation(root.Id, 0, 10, "Annotation comment");
        await _annotationsRepository.SaveAsync(annotation);

        var options = new TimelineOptions
        {
            SourceTypes = new HashSet<EventSourceType> 
            { 
                EventSourceType.Topics, 
                EventSourceType.Annotations 
            }
        };

        // Act
        var result = await _service.GenerateTimelineAsync(options);

        // Assert
        result.Should().NotBeNull();
        result.Events.Should().NotBeEmpty();
        result.Events.Should().Contain(e => e.EventType == TimelineEventType.TopicCreated);
        result.Events.Should().Contain(e => e.EventType == TimelineEventType.AnnotationCreated);
    }
}

