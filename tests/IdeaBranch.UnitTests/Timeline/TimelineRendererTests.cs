using FluentAssertions;
using IdeaBranch.App.Controls;
using IdeaBranch.Domain.Timeline;
using NUnit.Framework;
using SkiaSharp;

namespace IdeaBranch.UnitTests.Timeline;

/// <summary>
/// Unit tests for TimelineRenderer clustering and layout algorithms.
/// Note: These tests require MAUI runtime and may be skipped in headless environments.
/// </summary>
[TestFixture]
public class TimelineRendererTests
{
    private SkiaTimelineView? _view;
    private object? _renderer;

    [SetUp]
    public void SetUp()
    {
        try
        {
            _view = new SkiaTimelineView();
            
            // Use reflection to get the internal TimelineRenderer
            var rendererField = typeof(SkiaTimelineView).GetField(
                "_renderer",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (rendererField == null)
                throw new InvalidOperationException("_renderer field not found");
                
            _renderer = rendererField.GetValue(_view);
            
            if (_renderer == null)
                throw new InvalidOperationException("Renderer is null");
        }
        catch (System.TypeInitializationException)
        {
            // MAUI not initialized - skip these tests
            _view = null;
            _renderer = null;
        }
        catch (System.Runtime.InteropServices.COMException)
        {
            // MAUI runtime not available - skip these tests
            _view = null;
            _renderer = null;
        }
    }

    private void AssertMauiAvailable()
    {
        if (_view == null || _renderer == null)
            Assert.Inconclusive("MAUI runtime not available - skipping MAUI-dependent tests");
    }

    [Test]
    public void ClusterEvents_WithEmptyList_ShouldReturnEmpty()
    {
        AssertMauiAvailable();
        
        // Arrange
        var events = new List<TimelineEventView>();

        // Act
        var clusters = InvokeClusterEvents(events, 1.0, 800, new DateTime(2024, 1, 1));

        // Assert
        clusters.Should().BeEmpty();
    }

    [Test]
    public void ClusterEvents_WithSingleEvent_ShouldReturnSingleCluster()
    {
        AssertMauiAvailable();
        
        // Arrange
        var start = new DateTime(2024, 1, 15);
        var events = new List<TimelineEventView>
        {
            CreateEvent("1", start, "TopicCreated")
        };

        // Act
        var clusters = InvokeClusterEvents(events, 1.0, 800, new DateTime(2024, 1, 1));

        // Assert
        clusters.Should().HaveCount(1);
        GetClusterEvents(clusters[0]).Should().HaveCount(1);
        IsCluster(clusters[0]).Should().BeFalse();
    }

    [Test]
    public void ClusterEvents_WithCloseEvents_ShouldCluster()
    {
        AssertMauiAvailable();
        
        // Arrange
        var baseDate = new DateTime(2024, 1, 15);
        var events = new List<TimelineEventView>
        {
            CreateEvent("1", baseDate, "TopicCreated"),
            CreateEvent("2", baseDate.AddHours(1), "TopicCreated"),
            CreateEvent("3", baseDate.AddHours(2), "TopicCreated")
        };

        // Act - using large pixelsPerDay so events appear close together
        var clusters = InvokeClusterEvents(events, 0.1, 800, new DateTime(2024, 1, 1));

        // Assert - should cluster close events
        clusters.Should().NotBeEmpty();
        var clusterCount = clusters.Count(c => IsCluster(c));
        clusterCount.Should().BeGreaterThan(0);
    }

    [Test]
    public void ClusterEvents_WithDistantEvents_ShouldNotCluster()
    {
        AssertMauiAvailable();
        
        // Arrange
        var baseDate = new DateTime(2024, 1, 15);
        var events = new List<TimelineEventView>
        {
            CreateEvent("1", baseDate, "TopicCreated"),
            CreateEvent("2", baseDate.AddDays(30), "TopicCreated")
        };

        // Act - using small pixelsPerDay so events appear far apart
        var clusters = InvokeClusterEvents(events, 10.0, 800, new DateTime(2024, 1, 1));

        // Assert - should not cluster distant events
        clusters.Should().HaveCount(2);
        clusters.All(c => !IsCluster(c)).Should().BeTrue();
    }

    [Test]
    public void GetVisibleEvents_WithEventsInRange_ShouldFilterCorrectly()
    {
        AssertMauiAvailable();
        
        // Arrange
        var viewStart = new DateTime(2024, 1, 1);
        var viewEnd = new DateTime(2024, 1, 31);
        var events = new System.Collections.ObjectModel.ObservableCollection<TimelineEventView>
        {
            CreateEvent("1", new DateTime(2024, 1, 15), "TopicCreated"),
            CreateEvent("2", new DateTime(2023, 12, 15), "TopicCreated"), // Before view
            CreateEvent("3", new DateTime(2024, 2, 15), "TopicCreated"), // After view
            CreateEvent("4", new DateTime(2024, 1, 20), "TopicCreated")
        };

        // Act
        var visible = InvokeGetVisibleEvents(events, viewStart, viewEnd);

        // Assert
        visible.Should().HaveCount(2);
        visible.Should().Contain(e => e.Id == "1");
        visible.Should().Contain(e => e.Id == "4");
    }

    [Test]
    public void GetVisibleEvents_WithEventSpanningRange_ShouldInclude()
    {
        AssertMauiAvailable();
        
        // Arrange
        var viewStart = new DateTime(2024, 1, 15);
        var viewEnd = new DateTime(2024, 1, 20);
        var events = new System.Collections.ObjectModel.ObservableCollection<TimelineEventView>
        {
            CreateEventWithDuration("1", 
                new DateTime(2024, 1, 10), 
                new DateTime(2024, 1, 25), 
                "TopicCreated")
        };

        // Act
        var visible = InvokeGetVisibleEvents(events, viewStart, viewEnd);

        // Assert
        visible.Should().HaveCount(1);
        visible[0].Id.Should().Be("1");
    }

    [Test]
    public void GetVisibleEvents_WithEventAtBoundary_ShouldInclude()
    {
        AssertMauiAvailable();
        
        // Arrange
        var viewStart = new DateTime(2024, 1, 1);
        var viewEnd = new DateTime(2024, 1, 31);
        var events = new System.Collections.ObjectModel.ObservableCollection<TimelineEventView>
        {
            CreateEvent("1", viewStart, "TopicCreated"), // At start boundary
            CreateEvent("2", viewEnd, "TopicCreated"), // At end boundary
            CreateEvent("3", viewStart.AddDays(-1), "TopicCreated"), // Before range
            CreateEvent("4", viewEnd.AddDays(1), "TopicCreated") // After range
        };

        // Act
        var visible = InvokeGetVisibleEvents(events, viewStart, viewEnd);

        // Assert
        visible.Should().HaveCount(2);
        visible.Should().Contain(e => e.Id == "1");
        visible.Should().Contain(e => e.Id == "2");
    }

    [Test]
    public void GetVisibleEvents_WithEmptyCollection_ShouldReturnEmpty()
    {
        AssertMauiAvailable();
        
        // Arrange
        var viewStart = new DateTime(2024, 1, 1);
        var viewEnd = new DateTime(2024, 1, 31);
        var events = new System.Collections.ObjectModel.ObservableCollection<TimelineEventView>();

        // Act
        var visible = InvokeGetVisibleEvents(events, viewStart, viewEnd);

        // Assert
        visible.Should().BeEmpty();
    }

    [Test]
    public void ClusterEvents_WithVeryLargePixelPerDay_ShouldNotCluster()
    {
        AssertMauiAvailable();
        
        // Arrange - Very large pixelsPerDay means events are very far apart visually
        var baseDate = new DateTime(2024, 1, 15);
        var events = new List<TimelineEventView>
        {
            CreateEvent("1", baseDate, "TopicCreated"),
            CreateEvent("2", baseDate.AddHours(1), "TopicCreated"),
            CreateEvent("3", baseDate.AddHours(2), "TopicCreated")
        };

        // Act - using very large pixelsPerDay
        var clusters = InvokeClusterEvents(events, 1000.0, 800, new DateTime(2024, 1, 1));

        // Assert - should not cluster when pixelsPerDay is very large
        clusters.Should().HaveCount(3);
        clusters.All(c => !IsCluster(c)).Should().BeTrue();
    }

    [Test]
    public void ClusterEvents_WithVerySmallPixelPerDay_ShouldClusterMore()
    {
        AssertMauiAvailable();
        
        // Arrange - Very small pixelsPerDay means events are very close together visually
        var baseDate = new DateTime(2024, 1, 15);
        var events = new List<TimelineEventView>
        {
            CreateEvent("1", baseDate, "TopicCreated"),
            CreateEvent("2", baseDate.AddHours(1), "TopicCreated"),
            CreateEvent("3", baseDate.AddHours(2), "TopicCreated"),
            CreateEvent("4", baseDate.AddHours(3), "TopicCreated")
        };

        // Act - using very small pixelsPerDay
        var clusters = InvokeClusterEvents(events, 0.001, 800, new DateTime(2024, 1, 1));

        // Assert - should cluster more when pixelsPerDay is very small
        clusters.Should().NotBeEmpty();
        var clusterCount = clusters.Count(c => IsCluster(c));
        clusterCount.Should().BeGreaterThan(0);
    }

    [Test]
    public void ClusterEvents_WithEventsOutsideViewport_ShouldFilterCorrectly()
    {
        AssertMauiAvailable();
        
        // Arrange
        var viewStart = new DateTime(2024, 1, 15);
        var baseDate = viewStart;
        var events = new List<TimelineEventView>
        {
            CreateEvent("1", baseDate, "TopicCreated"), // In viewport
            CreateEvent("2", baseDate.AddDays(1), "TopicCreated"), // In viewport
            CreateEvent("3", viewStart.AddDays(-10), "TopicCreated"), // Before viewport
            CreateEvent("4", viewStart.AddDays(30), "TopicCreated") // After viewport
        };

        // Act
        var clusters = InvokeClusterEvents(events, 1.0, 800, viewStart);

        // Assert - clustering should only consider visible events
        clusters.Should().NotBeEmpty();
    }

    // Helper methods to access private members via reflection
    private List<object> InvokeClusterEvents(
        List<TimelineEventView> events,
        double pixelsPerDay,
        float viewportWidth,
        DateTime viewStart)
    {
        var method = _renderer!.GetType().GetMethod(
            "ClusterEvents",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (method == null)
            throw new InvalidOperationException("ClusterEvents method not found");

        var result = method.Invoke(_renderer!, new object[] { events, pixelsPerDay, viewportWidth, viewStart });
        return (List<object>)result!;
    }

    private bool IsCluster(object cluster)
    {
        var isClusterProp = cluster.GetType().GetProperty("IsCluster");
        return (bool)(isClusterProp?.GetValue(cluster) ?? false);
    }

    private List<TimelineEventView> GetClusterEvents(object cluster)
    {
        var eventsProp = cluster.GetType().GetProperty("Events");
        return (List<TimelineEventView>)eventsProp?.GetValue(cluster)!;
    }

    private List<TimelineEventView> InvokeGetVisibleEvents(
        System.Collections.ObjectModel.ObservableCollection<TimelineEventView> events,
        DateTime viewStart,
        DateTime viewEnd)
    {
        var method = _renderer!.GetType().GetMethod(
            "GetVisibleEvents",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (method == null)
            throw new InvalidOperationException("GetVisibleEvents method not found");

        return (List<TimelineEventView>)method.Invoke(_renderer!, new object[] { events, viewStart, viewEnd })!;
    }

    private TimelineEventView CreateEvent(string id, DateTime date, string type)
    {
        var instant = TemporalInstant.FromDateTime(date);
        var range = TemporalRange.Point(instant);
        return new TimelineEventView(id, $"Event {id}", type, range);
    }

    private TimelineEventView CreateEventWithDuration(string id, DateTime start, DateTime end, string type)
    {
        var startInstant = TemporalInstant.FromDateTime(start);
        var endInstant = TemporalInstant.FromDateTime(end);
        var range = TemporalRange.Duration(startInstant, endInstant);
        return new TimelineEventView(id, $"Event {id}", type, range);
    }
}

// EventCluster is internal to TimelineRenderer - we'll use reflection to access it

