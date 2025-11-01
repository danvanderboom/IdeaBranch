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
}

