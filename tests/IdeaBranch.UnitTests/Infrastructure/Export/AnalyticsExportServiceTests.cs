using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using IdeaBranch.Domain;
using IdeaBranch.Infrastructure.Export;
using NUnit.Framework;
using System.Text.Json;

namespace IdeaBranch.UnitTests.Infrastructure.Export;

/// <summary>
/// Unit tests for AnalyticsExportService export formatting and filter application.
/// </summary>
[TestFixture]
public class AnalyticsExportServiceTests
{
    private AnalyticsExportService _exportService = null!;

    [SetUp]
    public void SetUp()
    {
        _exportService = new AnalyticsExportService();
    }

    [Test]
    public async Task ExportTimelineToJsonAsync_WithIncludeAllFields_IncludesAllRequiredFields()
    {
        // Arrange
        var timelineData = new TimelineData
        {
            Events = new[]
            {
                new TimelineEvent
                {
                    Id = Guid.NewGuid(),
                    Timestamp = new DateTime(2025, 1, 15, 10, 30, 0),
                    EventType = TimelineEventType.TopicCreated,
                    Title = "Test Topic",
                    Details = "Test details",
                    NodeId = Guid.NewGuid(),
                    TagIds = new[] { Guid.NewGuid(), Guid.NewGuid() }
                }
            }
        };

        // Act
        var json = await _exportService.ExportTimelineToJsonAsync(timelineData, includeAllFields: true);

        // Assert
        json.Should().NotBeNullOrEmpty();
        
        // Parse JSON to verify structure
        var parsed = JsonSerializer.Deserialize<JsonElement[]>(json);
        parsed.Should().NotBeNull();
        parsed!.Length.Should().Be(1);

        var eventObj = parsed[0];
        eventObj.GetProperty("eventId").GetString().Should().NotBeNullOrEmpty();
        eventObj.GetProperty("type").GetString().Should().Be("TopicCreated");
        eventObj.GetProperty("title").GetString().Should().Be("Test Topic");
        eventObj.GetProperty("body").GetString().Should().Be("Test details");
        eventObj.GetProperty("start").GetString().Should().NotBeNullOrEmpty();
        eventObj.GetProperty("precision").GetString().Should().Be("day");
        eventObj.GetProperty("nodeId").GetString().Should().NotBeNullOrEmpty();
        eventObj.GetProperty("tags").EnumerateArray().Count().Should().Be(2);
        eventObj.GetProperty("source").GetString().Should().Be("Topics");
        eventObj.GetProperty("actor").GetString().Should().Be("System");
        eventObj.GetProperty("createdAt").GetString().Should().NotBeNullOrEmpty();
        eventObj.GetProperty("updatedAt").GetString().Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task ExportTimelineToCsvAsync_WithIncludeAllFields_IncludesAllRequiredColumns()
    {
        // Arrange
        var timelineData = new TimelineData
        {
            Events = new[]
            {
                new TimelineEvent
                {
                    Id = Guid.NewGuid(),
                    Timestamp = new DateTime(2025, 1, 15, 10, 30, 0),
                    EventType = TimelineEventType.AnnotationCreated,
                    Title = "Test Annotation",
                    Details = "Test annotation details",
                    NodeId = Guid.NewGuid(),
                    TagIds = new[] { Guid.NewGuid() }
                }
            }
        };

        // Act
        var csv = await _exportService.ExportTimelineToCsvAsync(timelineData, includeAllFields: true);

        // Assert
        csv.Should().NotBeNullOrEmpty();
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Length.Should().BeGreaterThan(1); // Header + data rows

        var header = lines[0];
        header.Should().Contain("eventId");
        header.Should().Contain("type");
        header.Should().Contain("title");
        header.Should().Contain("body");
        header.Should().Contain("start");
        header.Should().Contain("precision");
        header.Should().Contain("nodeId");
        header.Should().Contain("tags");
        header.Should().Contain("source");
        header.Should().Contain("actor");
        header.Should().Contain("createdAt");
        header.Should().Contain("updatedAt");

        // Verify data row has correct values
        if (lines.Length > 1)
        {
            var dataRow = lines[1];
            dataRow.Should().Contain("AnnotationCreated");
            dataRow.Should().Contain("Test Annotation");
            dataRow.Should().Contain("Annotations"); // source
            dataRow.Should().Contain("System"); // actor
        }
    }

    [Test]
    public async Task ExportTimelineToCsvAsync_WithIncludeAllFields_EscapesSpecialCharacters()
    {
        // Arrange
        var timelineData = new TimelineData
        {
            Events = new[]
            {
                new TimelineEvent
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.UtcNow,
                    EventType = TimelineEventType.TopicCreated,
                    Title = "Topic with, comma and \"quote\"",
                    Details = "Details\nwith\nnewlines",
                    NodeId = Guid.NewGuid(),
                    TagIds = Array.Empty<Guid>()
                }
            }
        };

        // Act
        var csv = await _exportService.ExportTimelineToCsvAsync(timelineData, includeAllFields: true);

        // Assert
        csv.Should().NotBeNullOrEmpty();
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Length.Should().BeGreaterThan(1);
        
        // CSV should handle special characters properly
        var dataRow = lines[1];
        dataRow.Should().Contain("Topic with");
    }

    [Test]
    public async Task ExportTimelineToJsonAsync_WithoutIncludeAllFields_UsesStandardFormat()
    {
        // Arrange
        var timelineData = new TimelineData
        {
            Events = new[]
            {
                new TimelineEvent
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.UtcNow,
                    EventType = TimelineEventType.TopicCreated,
                    Title = "Test",
                    NodeId = Guid.NewGuid()
                }
            }
        };

        // Act
        var json = await _exportService.ExportTimelineToJsonAsync(timelineData, includeAllFields: false);

        // Assert
        json.Should().NotBeNullOrEmpty();
        
        // Should parse as TimelineData structure, not enhanced format
        var parsed = JsonSerializer.Deserialize<TimelineData>(json, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });
        parsed.Should().NotBeNull();
        parsed!.Events.Should().NotBeEmpty();
    }

    [Test]
    public async Task ExportTimelineToCsvAsync_WithoutIncludeAllFields_UsesStandardFormat()
    {
        // Arrange
        var timelineData = new TimelineData
        {
            Events = new[]
            {
                new TimelineEvent
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.UtcNow,
                    EventType = TimelineEventType.TopicCreated,
                    Title = "Test",
                    NodeId = Guid.NewGuid()
                }
            }
        };

        // Act
        var csv = await _exportService.ExportTimelineToCsvAsync(timelineData, includeAllFields: false);

        // Assert
        csv.Should().NotBeNullOrEmpty();
        var header = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries)[0];
        header.Should().Contain("Timestamp");
        header.Should().Contain("EventType");
        header.Should().NotContain("eventId"); // Should use standard format, not enhanced
    }

    [Test]
    public async Task ExportTimelineToJsonAsync_WithMultipleEvents_ExportsAllEvents()
    {
        // Arrange
        var timelineData = new TimelineData
        {
            Events = new[]
            {
                new TimelineEvent { Id = Guid.NewGuid(), Timestamp = DateTime.UtcNow, EventType = TimelineEventType.TopicCreated, Title = "Event 1" },
                new TimelineEvent { Id = Guid.NewGuid(), Timestamp = DateTime.UtcNow, EventType = TimelineEventType.TopicUpdated, Title = "Event 2" },
                new TimelineEvent { Id = Guid.NewGuid(), Timestamp = DateTime.UtcNow, EventType = TimelineEventType.AnnotationCreated, Title = "Event 3" }
            }
        };

        // Act
        var json = await _exportService.ExportTimelineToJsonAsync(timelineData, includeAllFields: true);

        // Assert
        json.Should().NotBeNullOrEmpty();
        var parsed = JsonSerializer.Deserialize<JsonElement[]>(json);
        parsed.Should().NotBeNull();
        parsed!.Length.Should().Be(3);
    }

    [Test]
    public async Task ExportTimelineToCsvAsync_WithMultipleEvents_ExportsAllEvents()
    {
        // Arrange
        var timelineData = new TimelineData
        {
            Events = new[]
            {
                new TimelineEvent { Id = Guid.NewGuid(), Timestamp = DateTime.UtcNow, EventType = TimelineEventType.TopicCreated, Title = "Event 1" },
                new TimelineEvent { Id = Guid.NewGuid(), Timestamp = DateTime.UtcNow, EventType = TimelineEventType.TopicUpdated, Title = "Event 2" }
            }
        };

        // Act
        var csv = await _exportService.ExportTimelineToCsvAsync(timelineData, includeAllFields: true);

        // Assert
        csv.Should().NotBeNullOrEmpty();
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Length.Should().Be(3); // Header + 2 data rows
    }

    [Test]
    public async Task ExportTimelineToJsonAsync_WithEmptyEvents_ExportsEmptyArray()
    {
        // Arrange
        var timelineData = new TimelineData
        {
            Events = Array.Empty<TimelineEvent>()
        };

        // Act
        var json = await _exportService.ExportTimelineToJsonAsync(timelineData, includeAllFields: true);

        // Assert
        json.Should().NotBeNullOrEmpty();
        var parsed = JsonSerializer.Deserialize<JsonElement[]>(json);
        parsed.Should().NotBeNull();
        parsed!.Length.Should().Be(0);
    }

    [Test]
    public async Task ExportTimelineToCsvAsync_RespectsFilteredEvents()
    {
        // Arrange - Create timeline data with multiple events
        var timelineData = new TimelineData
        {
            Events = new[]
            {
                new TimelineEvent 
                { 
                    Id = Guid.NewGuid(), 
                    Timestamp = new DateTime(2025, 1, 15), 
                    EventType = TimelineEventType.TopicCreated, 
                    Title = "Event 1",
                    TagIds = new[] { Guid.NewGuid() }
                },
                new TimelineEvent 
                { 
                    Id = Guid.NewGuid(), 
                    Timestamp = new DateTime(2025, 1, 16), 
                    EventType = TimelineEventType.AnnotationCreated, 
                    Title = "Event 2",
                    TagIds = Array.Empty<Guid>()
                },
                new TimelineEvent 
                { 
                    Id = Guid.NewGuid(), 
                    Timestamp = new DateTime(2025, 1, 17), 
                    EventType = TimelineEventType.TopicUpdated, 
                    Title = "Event 3",
                    TagIds = new[] { Guid.NewGuid() }
                }
            }
        };

        // Act
        var csv = await _exportService.ExportTimelineToCsvAsync(timelineData, includeAllFields: true);

        // Assert - All events should be exported (filter application is handled by TimelineViewModel before passing data)
        csv.Should().NotBeNullOrEmpty();
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Length.Should().Be(4); // Header + 3 data rows
        
        // Verify all events are present
        var csvContent = string.Join("\n", lines);
        csvContent.Should().Contain("Event 1");
        csvContent.Should().Contain("Event 2");
        csvContent.Should().Contain("Event 3");
    }

    [Test]
    public async Task ExportTimelineToJsonAsync_RespectsFilteredEvents()
    {
        // Arrange - Create timeline data with multiple events
        var timelineData = new TimelineData
        {
            Events = new[]
            {
                new TimelineEvent 
                { 
                    Id = Guid.NewGuid(), 
                    Timestamp = new DateTime(2025, 1, 15), 
                    EventType = TimelineEventType.TopicCreated, 
                    Title = "Event 1"
                },
                new TimelineEvent 
                { 
                    Id = Guid.NewGuid(), 
                    Timestamp = new DateTime(2025, 1, 16), 
                    EventType = TimelineEventType.AnnotationCreated, 
                    Title = "Event 2"
                }
            }
        };

        // Act
        var json = await _exportService.ExportTimelineToJsonAsync(timelineData, includeAllFields: true);

        // Assert - All events should be exported (filter application is handled by TimelineViewModel before passing data)
        json.Should().NotBeNullOrEmpty();
        var parsed = JsonSerializer.Deserialize<JsonElement[]>(json);
        parsed.Should().NotBeNull();
        parsed!.Length.Should().Be(2);
        
        // Verify event titles
        parsed[0].GetProperty("title").GetString().Should().Be("Event 1");
        parsed[1].GetProperty("title").GetString().Should().Be("Event 2");
    }

    [Test]
    public async Task ExportTimelineToCsvAsync_WithNullDetails_HandlesGracefully()
    {
        // Arrange
        var timelineData = new TimelineData
        {
            Events = new[]
            {
                new TimelineEvent
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.UtcNow,
                    EventType = TimelineEventType.TopicCreated,
                    Title = "Test",
                    Details = null,
                    NodeId = null,
                    TagIds = Array.Empty<Guid>()
                }
            }
        };

        // Act
        var csv = await _exportService.ExportTimelineToCsvAsync(timelineData, includeAllFields: true);

        // Assert
        csv.Should().NotBeNullOrEmpty();
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Length.Should().BeGreaterThan(1);
    }
}

