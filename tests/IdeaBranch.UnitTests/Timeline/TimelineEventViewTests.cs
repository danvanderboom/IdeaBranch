using FluentAssertions;
using IdeaBranch.Domain;
using IdeaBranch.Domain.Timeline;

namespace IdeaBranch.UnitTests.Timeline;

public class TimelineEventViewTests
{
    [Test]
    public void FromDomainEvent_ShouldConvertCorrectly()
    {
        // Arrange
        var domainEvent = new IdeaBranch.Domain.TimelineEvent
        {
            Id = Guid.Parse("12345678-1234-1234-1234-123456789abc"),
            Timestamp = new DateTime(2024, 3, 15, 14, 30, 0),
            EventType = TimelineEventType.TopicCreated,
            Title = "Test Topic",
            Details = "Test Details",
            NodeId = Guid.Parse("87654321-4321-4321-4321-cba987654321"),
            TagIds = new List<Guid> { Guid.Parse("11111111-1111-1111-1111-111111111111") }
        };

        // Act
        var view = TimelineEventView.FromDomainEvent(domainEvent);

        // Assert
        view.Id.Should().Be("12345678-1234-1234-1234-123456789abc");
        view.Title.Should().Be("Test Topic");
        view.Type.Should().Be("TopicCreated");
        view.When.Start.Date.Should().Be(new DateTime(2024, 3, 15));
        view.When.End.Should().BeNull();
        view.NodeId.Should().Be(domainEvent.NodeId);
        view.Details.Should().Be("Test Details");
        view.Tags.Should().NotBeNull();
        view.Tags!.Count.Should().Be(1);
    }

    [Test]
    public void FromDomainEvent_WithDifferentEventTypes_ShouldMapCorrectly()
    {
        // Arrange & Act
        var topicUpdated = TimelineEventView.FromDomainEvent(new IdeaBranch.Domain.TimelineEvent
        {
            EventType = TimelineEventType.TopicUpdated,
            Timestamp = DateTime.Now
        });

        var annotationCreated = TimelineEventView.FromDomainEvent(new IdeaBranch.Domain.TimelineEvent
        {
            EventType = TimelineEventType.AnnotationCreated,
            Timestamp = DateTime.Now
        });

        var conversationMessage = TimelineEventView.FromDomainEvent(new IdeaBranch.Domain.TimelineEvent
        {
            EventType = TimelineEventType.ConversationMessage,
            Timestamp = DateTime.Now
        });

        // Assert
        topicUpdated.Type.Should().Be("TopicUpdated");
        annotationCreated.Type.Should().Be("AnnotationCreated");
        conversationMessage.Type.Should().Be("ConversationMessage");
    }

    [Test]
    public void FromDomainEvent_WithNullNodeId_ShouldHandleCorrectly()
    {
        // Arrange
        var domainEvent = new IdeaBranch.Domain.TimelineEvent
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.Now,
            EventType = TimelineEventType.TopicCreated,
            Title = "Test",
            NodeId = null
        };

        // Act
        var view = TimelineEventView.FromDomainEvent(domainEvent);

        // Assert
        view.NodeId.Should().BeNull();
    }

    [Test]
    public void FromDomainEvent_WithEmptyTagIds_ShouldSetTagsToNull()
    {
        // Arrange
        var domainEvent = new IdeaBranch.Domain.TimelineEvent
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.Now,
            EventType = TimelineEventType.TopicCreated,
            Title = "Test",
            TagIds = new List<Guid>()
        };

        // Act
        var view = TimelineEventView.FromDomainEvent(domainEvent);

        // Assert
        view.Tags.Should().BeNull();
    }

    [Test]
    public void FromDomainEvent_WithMultipleTags_ShouldConvertToDictionary()
    {
        // Arrange
        var tagId1 = Guid.NewGuid();
        var tagId2 = Guid.NewGuid();
        var domainEvent = new IdeaBranch.Domain.TimelineEvent
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.Now,
            EventType = TimelineEventType.TopicCreated,
            Title = "Test",
            TagIds = new List<Guid> { tagId1, tagId2 }
        };

        // Act
        var view = TimelineEventView.FromDomainEvent(domainEvent);

        // Assert
        view.Tags.Should().NotBeNull();
        view.Tags!.Count.Should().Be(2);
        view.Tags.Should().ContainKey(tagId1.ToString());
        view.Tags.Should().ContainKey(tagId2.ToString());
    }

    [Test]
    public void FromDomainEvent_WithNullDetails_ShouldSetDetailsToNull()
    {
        // Arrange
        var domainEvent = new IdeaBranch.Domain.TimelineEvent
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.Now,
            EventType = TimelineEventType.TopicCreated,
            Title = "Test",
            Details = null
        };

        // Act
        var view = TimelineEventView.FromDomainEvent(domainEvent);

        // Assert
        view.Details.Should().BeNull();
    }

    [Test]
    public void Constructor_WithAllParameters_ShouldSetAllProperties()
    {
        // Arrange
        var id = "test-id";
        var title = "Test Title";
        var type = "TestType";
        var when = TemporalRange.Point(DateTime.Now);
        var tags = new Dictionary<string, string> { { "key", "value" } };
        var nodeId = Guid.NewGuid();
        var details = "Test Details";

        // Act
        var view = new TimelineEventView(id, title, type, when, tags, nodeId, details);

        // Assert
        view.Id.Should().Be(id);
        view.Title.Should().Be(title);
        view.Type.Should().Be(type);
        view.When.Should().Be(when);
        view.Tags.Should().BeEquivalentTo(tags);
        view.NodeId.Should().Be(nodeId);
        view.Details.Should().Be(details);
    }

    [Test]
    public void Constructor_WithMinimalParameters_ShouldSetDefaults()
    {
        // Arrange
        var id = "test-id";
        var title = "Test Title";
        var type = "TestType";
        var when = TemporalRange.Point(DateTime.Now);

        // Act
        var view = new TimelineEventView(id, title, type, when);

        // Assert
        view.Id.Should().Be(id);
        view.Title.Should().Be(title);
        view.Type.Should().Be(type);
        view.When.Should().Be(when);
        view.Tags.Should().BeNull();
        view.NodeId.Should().BeNull();
        view.Details.Should().BeNull();
    }

    [Test]
    public void FromDomainEvent_WithDifferentPrecisions_ShouldUseDayPrecision()
    {
        // Arrange
        var domainEvent = new IdeaBranch.Domain.TimelineEvent
        {
            Id = Guid.NewGuid(),
            Timestamp = new DateTime(2024, 3, 15, 14, 30, 0),
            EventType = TimelineEventType.TopicCreated,
            Title = "Test"
        };

        // Act
        var view = TimelineEventView.FromDomainEvent(domainEvent);

        // Assert
        view.When.Start.Precision.Should().Be(TemporalPrecision.Day);
        view.When.Start.Date.Should().Be(new DateTime(2024, 3, 15));
    }

    [Test]
    public void FromDomainEvent_WithUnknownEventType_ShouldMapToUnknown()
    {
        // Arrange
        var domainEvent = new IdeaBranch.Domain.TimelineEvent
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.Now,
            EventType = (IdeaBranch.Domain.TimelineEventType)(-1), // Unknown enum value
            Title = "Test"
        };

        // Act
        var view = TimelineEventView.FromDomainEvent(domainEvent);

        // Assert
        view.Type.Should().Be("Unknown");
    }

    [Test]
    public void RecordEquality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var when = TemporalRange.Point(new DateTime(2024, 3, 15));
        var event1 = new TimelineEventView("1", "Title", "Type", when);
        var event2 = new TimelineEventView("1", "Title", "Type", when);

        // Assert
        event1.Should().BeEquivalentTo(event2);
        (event1 == event2).Should().BeTrue();
    }

    [Test]
    public void RecordEquality_WithDifferentIds_ShouldNotBeEqual()
    {
        // Arrange
        var when = TemporalRange.Point(new DateTime(2024, 3, 15));
        var event1 = new TimelineEventView("1", "Title", "Type", when);
        var event2 = new TimelineEventView("2", "Title", "Type", when);

        // Assert
        event1.Should().NotBeEquivalentTo(event2);
        (event1 == event2).Should().BeFalse();
    }
}

