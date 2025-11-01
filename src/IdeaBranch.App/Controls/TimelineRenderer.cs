using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using IdeaBranch.Domain.Timeline;
using SkiaSharp;

namespace IdeaBranch.App.Controls;

/// <summary>
/// Handles rendering of timeline events using SkiaSharp.
/// </summary>
internal class TimelineRenderer
{
    private readonly SkiaTimelineView _view;
    private static readonly Dictionary<string, SKColor> EventTypeColors = new()
    {
        { "TopicCreated", new SKColor(0xFF, 0x42, 0x85, 0xF4) },
        { "TopicUpdated", new SKColor(0x66, 0x42, 0x85, 0xF4) },
        { "AnnotationCreated", new SKColor(0xFF, 0x34, 0xA8, 0x53) },
        { "AnnotationUpdated", new SKColor(0x66, 0x34, 0xA8, 0x53) },
        { "ConversationMessage", new SKColor(0xFF, 0xEA, 0x43, 0x35) },
    };

    private static readonly Dictionary<string, float> EventTypeSizes = new()
    {
        { "TopicCreated", 12.0f },
        { "TopicUpdated", 8.0f },
        { "AnnotationCreated", 10.0f },
        { "AnnotationUpdated", 6.0f },
        { "ConversationMessage", 8.0f },
    };

    public TimelineRenderer(SkiaTimelineView view)
    {
        _view = view;
    }

    public void DrawTimeline(
        SKCanvas canvas,
        SKImageInfo info,
        ObservableCollection<TimelineEventView> events,
        DateTime viewStart,
        DateTime viewEnd,
        double zoomLevel,
        bool showDiagnostics = false,
        double fps = 0,
        double drawTimeMs = 0)
    {
        var timeRange = (viewEnd - viewStart).TotalDays;
        if (timeRange <= 0) return;

        var pixelsPerDay = info.Width / timeRange;

        // Draw time axis
        DrawTimeAxis(canvas, info, viewStart, viewEnd, pixelsPerDay);

        // Draw grid lines
        DrawGridLines(canvas, info, viewStart, viewEnd, pixelsPerDay);

        // Get visible events and clusters
        var visibleEvents = GetVisibleEvents(events, viewStart, viewEnd);
        var clusters = ClusterEvents(visibleEvents, pixelsPerDay, info.Width, viewStart);

        // Draw events/clusters
        foreach (var cluster in clusters)
        {
            if (cluster.IsCluster && cluster.Events.Count > 0)
            {
                DrawCluster(canvas, cluster, pixelsPerDay, viewStart);
            }
            else if (cluster.Events.Count == 1)
            {
                DrawEvent(canvas, cluster.Events[0], pixelsPerDay, viewStart);
            }
        }

        // Draw legend
        DrawLegend(canvas, info);

        // Draw diagnostics overlay if enabled
        if (showDiagnostics)
        {
            DrawDiagnostics(canvas, info, fps, drawTimeMs, events.Count, zoomLevel);
        }
    }

    public void DrawEmptyState(SKCanvas canvas, SKImageInfo info)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.Gray,
            TextSize = 16,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        };

        var text = "No timeline events. Generate timeline to view events.";
        var y = info.Height / 2;
        canvas.DrawText(text, info.Width / 2, y, paint);
    }

    private void DrawTimeAxis(SKCanvas canvas, SKImageInfo info, DateTime start, DateTime end, double pixelsPerDay)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            StrokeWidth = 2,
            IsAntialias = true
        };

        var axisY = info.Height - 30;
        canvas.DrawLine(0, axisY, info.Width, axisY, paint);

        // Draw time labels
        DrawTimeLabels(canvas, info, start, end, pixelsPerDay, axisY);
    }

    private void DrawTimeLabels(SKCanvas canvas, SKImageInfo info, DateTime start, DateTime end, double pixelsPerDay, float axisY)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            TextSize = 10,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        };

        var timeRange = (end - start).TotalDays;
        var labelInterval = CalculateLabelInterval(timeRange);

        var currentTime = AlignTime(start, labelInterval);
        while (currentTime <= end)
        {
            var x = (float)((currentTime - start).TotalDays * pixelsPerDay);
            if (x >= 0 && x <= info.Width)
            {
                canvas.DrawLine(x, axisY, x, axisY + 5, paint);
                var label = FormatTimeLabel(currentTime, labelInterval);
                canvas.DrawText(label, x, axisY + 20, paint);
            }

            currentTime = AddInterval(currentTime, labelInterval);
        }
    }

    private TimeInterval CalculateLabelInterval(double days)
    {
        if (days > 365) return TimeInterval.Year;
        if (days > 30) return TimeInterval.Month;
        if (days > 7) return TimeInterval.Week;
        return TimeInterval.Day;
    }

    private DateTime AlignTime(DateTime time, TimeInterval interval)
    {
        return interval switch
        {
            TimeInterval.Year => new DateTime(time.Year, 1, 1),
            TimeInterval.Month => new DateTime(time.Year, time.Month, 1),
            TimeInterval.Week => time.Date.AddDays(-(int)time.DayOfWeek),
            TimeInterval.Day => time.Date,
            _ => time.Date
        };
    }

    private DateTime AddInterval(DateTime time, TimeInterval interval)
    {
        return interval switch
        {
            TimeInterval.Year => time.AddYears(1),
            TimeInterval.Month => time.AddMonths(1),
            TimeInterval.Week => time.AddDays(7),
            TimeInterval.Day => time.AddDays(1),
            _ => time.AddDays(1)
        };
    }

    private string FormatTimeLabel(DateTime time, TimeInterval interval)
    {
        return interval switch
        {
            TimeInterval.Year => time.ToString("yyyy"),
            TimeInterval.Month => time.ToString("MMM yyyy"),
            TimeInterval.Week => time.ToString("MMM dd"),
            TimeInterval.Day => time.ToString("MM/dd"),
            _ => time.ToString("MM/dd")
        };
    }

    private void DrawGridLines(SKCanvas canvas, SKImageInfo info, DateTime start, DateTime end, double pixelsPerDay)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.LightGray,
            StrokeWidth = 1,
            IsAntialias = true
        };

        var timeRange = (end - start).TotalDays;
        var interval = CalculateLabelInterval(timeRange);
        var currentTime = AlignTime(start, interval);

        while (currentTime <= end)
        {
            var x = (float)((currentTime - start).TotalDays * pixelsPerDay);
            if (x >= 0 && x <= info.Width)
            {
                canvas.DrawLine(x, 0, x, info.Height - 30, paint);
            }

            currentTime = AddInterval(currentTime, interval);
        }
    }

    private List<TimelineEventView> GetVisibleEvents(
        ObservableCollection<TimelineEventView> events,
        DateTime viewStart,
        DateTime viewEnd)
    {
        var visible = new List<TimelineEventView>();

        foreach (var evt in events)
        {
            var eventStart = evt.When.Start.Date;
            var eventEnd = evt.When.End?.Date ?? eventStart;

            // Check if event overlaps with viewport
            if (eventEnd >= viewStart && eventStart <= viewEnd)
            {
                visible.Add(evt);
            }
        }

        return visible;
    }

    private List<EventCluster> ClusterEvents(
        List<TimelineEventView> events,
        double pixelsPerDay,
        float viewportWidth,
        DateTime viewStart)
    {
        if (events.Count == 0) return new List<EventCluster>();

        var clusters = new List<EventCluster>();
        const float clusterThreshold = 20.0f; // pixels

        // Sort events by time
        var sorted = events.OrderBy(e => e.When.Start.Date).ToList();

        var currentCluster = new List<TimelineEventView> { sorted[0] };
        var lastX = (float)((sorted[0].When.Start.Date - viewStart).TotalDays * pixelsPerDay);

        for (int i = 1; i < sorted.Count; i++)
        {
            var evt = sorted[i];
            var x = (float)((evt.When.Start.Date - viewStart).TotalDays * pixelsPerDay);

            if (Math.Abs(x - lastX) < clusterThreshold)
            {
                currentCluster.Add(evt);
            }
            else
            {
                clusters.Add(new EventCluster
                {
                    Events = currentCluster.ToList(),
                    IsCluster = currentCluster.Count > 1
                });
                currentCluster = new List<TimelineEventView> { evt };
            }

            lastX = x;
        }

        if (currentCluster.Count > 0)
        {
            clusters.Add(new EventCluster
            {
                Events = currentCluster.ToList(),
                IsCluster = currentCluster.Count > 1
            });
        }

        return clusters;
    }

    private void DrawEvent(SKCanvas canvas, TimelineEventView evt, double pixelsPerDay, DateTime viewStart)
    {
        var x = (float)((evt.When.Start.Date - viewStart).TotalDays * pixelsPerDay);
        var y = 50.0f; // Base Y position for events

        var color = EventTypeColors.GetValueOrDefault(evt.Type, SKColors.Gray);
        var size = EventTypeSizes.GetValueOrDefault(evt.Type, 8.0f);

        using var paint = new SKPaint
        {
            Color = color,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        canvas.DrawCircle(x, y, size, paint);
    }

    private void DrawCluster(SKCanvas canvas, EventCluster cluster, double pixelsPerDay, DateTime viewStart)
    {
        if (cluster.Events.Count == 0) return;

        var firstEvent = cluster.Events.OrderBy(e => e.When.Start.Date).First();
        var x = (float)((firstEvent.When.Start.Date - viewStart).TotalDays * pixelsPerDay);
        var y = 50.0f;

        using var paint = new SKPaint
        {
            Color = SKColors.DarkGray,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        var size = 15.0f;
        canvas.DrawCircle(x, y, size, paint);

        // Draw count label
        using var textPaint = new SKPaint
        {
            Color = SKColors.White,
            TextSize = 10,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        };

        canvas.DrawText(cluster.Events.Count.ToString(), x, y + 4, textPaint);
    }

    private void DrawLegend(SKCanvas canvas, SKImageInfo info)
    {
        var legendX = 10.0f;
        var legendY = 10.0f;
        var spacing = 20.0f;

        using var textPaint = new SKPaint
        {
            Color = SKColors.Black,
            TextSize = 12,
            IsAntialias = true
        };

        foreach (var kvp in EventTypeColors)
        {
            using var circlePaint = new SKPaint
            {
                Color = kvp.Value,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            var size = EventTypeSizes.GetValueOrDefault(kvp.Key, 8.0f);
            canvas.DrawCircle(legendX, legendY, size, circlePaint);

            var labelX = legendX + size + 5;
            canvas.DrawText(kvp.Key, labelX, legendY + 4, textPaint);

            legendY += spacing;
        }
    }

    private void DrawDiagnostics(SKCanvas canvas, SKImageInfo info, double fps, double drawTimeMs, int eventCount, double zoomLevel)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            TextSize = 12,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        using var bgPaint = new SKPaint
        {
            Color = new SKColor(0xE0, 0xFF, 0xFF, 0xFF), // Semi-transparent white background
            Style = SKPaintStyle.Fill
        };

        var padding = 5.0f;
        var lineHeight = 15.0f;
        var lines = new[]
        {
            $"FPS: {fps:F1}",
            $"Draw: {drawTimeMs:F2}ms",
            $"Events: {eventCount}",
            $"Zoom: {zoomLevel:F2}x"
        };

        var maxWidth = lines.Max(l => paint.MeasureText(l));
        var boxHeight = (lines.Length + 1) * lineHeight;
        var boxRect = new SKRect(padding, padding, padding + maxWidth + 10, padding + boxHeight);

        canvas.DrawRect(boxRect, bgPaint);
        
        var y = padding + lineHeight;
        foreach (var line in lines)
        {
            canvas.DrawText(line, padding + 5, y, paint);
            y += lineHeight;
        }
    }
}

internal class EventCluster
{
    public required List<TimelineEventView> Events { get; init; }
    public bool IsCluster { get; init; }
}

internal enum TimeInterval
{
    Day,
    Week,
    Month,
    Year
}

