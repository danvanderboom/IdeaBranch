using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using IdeaBranch.App.Services;
using IdeaBranch.Domain.Timeline;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Graphics;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace IdeaBranch.App.Controls;

/// <summary>
/// Interactive SkiaSharp-based timeline control with zoom, pan, clustering, and filtering.
/// </summary>
public partial class SkiaTimelineView : ContentView
{
    private readonly TimelineRenderer _renderer;
    private readonly TimelineInteractionHandler _interactionHandler;
    private DateTime _viewStartTime;
    private DateTime _viewEndTime;
    private double _zoomLevel = 1.0;

    // Telemetry
    private readonly Stopwatch _frameStopwatch = Stopwatch.StartNew();
    private readonly Queue<double> _frameTimes = new(60); // Keep last 60 frames
    private int _frameCount = 0;
    private DateTime _lastTelemetryEmit = DateTime.UtcNow;
    private const int TelemetryEmitIntervalSeconds = 5; // Emit telemetry every 5 seconds
    private bool _showDiagnostics = false;

    /// <summary>
    /// Identifies the Events bindable property.
    /// </summary>
    public static readonly BindableProperty EventsProperty = BindableProperty.Create(
        nameof(Events),
        typeof(ObservableCollection<TimelineEventView>),
        typeof(SkiaTimelineView),
        default(ObservableCollection<TimelineEventView>),
        propertyChanged: OnEventsChanged);

    private static void OnEventsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SkiaTimelineView view)
        {
            if (oldValue is ObservableCollection<TimelineEventView> oldCollection)
            {
                oldCollection.CollectionChanged -= view.OnEventsCollectionChanged;
            }
            if (newValue is ObservableCollection<TimelineEventView> newCollection)
            {
                newCollection.CollectionChanged += view.OnEventsCollectionChanged;
            }
            view.InvalidateCanvas();
        }
    }

    private void OnEventsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        InvalidateCanvas();
    }

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    public SkiaTimelineView()
    {
        InitializeComponent();
        _renderer = new TimelineRenderer(this);
        _interactionHandler = new TimelineInteractionHandler(this, _renderer);
        
        // Set default time range (last year)
        var now = DateTime.Now;
        _viewStartTime = now.AddYears(-1);
        _viewEndTime = now;
    }

    /// <summary>
    /// Gets or sets the events to display.
    /// </summary>
    public ObservableCollection<TimelineEventView>? Events
    {
        get => (ObservableCollection<TimelineEventView>?)GetValue(EventsProperty);
        set => SetValue(EventsProperty, value);
    }

    /// <summary>
    /// Gets or sets the selected time range for filtering.
    /// </summary>
    public (DateTime Start, DateTime End)? SelectedTimeRange
    {
        get => _interactionHandler.SelectedRange;
        set
        {
            _interactionHandler.SelectedRange = value;
            SelectedTimeRangeChanged?.Invoke(this, value);
            InvalidateCanvas();
        }
    }

    /// <summary>
    /// Gets or sets the selected event.
    /// </summary>
    public TimelineEventView? SelectedEvent
    {
        get => _interactionHandler.SelectedEvent;
        set
        {
            _interactionHandler.SelectedEvent = value;
            SelectedEventChanged?.Invoke(this, value);
            InvalidateCanvas();
        }
    }

    /// <summary>
    /// Identifies the GroupByType bindable property.
    /// </summary>
    public static readonly BindableProperty GroupByTypeProperty = BindableProperty.Create(
        nameof(GroupByType),
        typeof(bool),
        typeof(SkiaTimelineView),
        false,
        propertyChanged: OnGroupByTypeChanged);

    private static void OnGroupByTypeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SkiaTimelineView view)
        {
            view.InvalidateCanvas();
        }
    }

    /// <summary>
    /// Gets or sets whether to group events by type into bands.
    /// </summary>
    public bool GroupByType
    {
        get => (bool)GetValue(GroupByTypeProperty);
        set => SetValue(GroupByTypeProperty, value);
    }

    /// <summary>
    /// Occurs when the selected time range changes.
    /// </summary>
    public event EventHandler<(DateTime Start, DateTime End)?>? SelectedTimeRangeChanged;

    /// <summary>
    /// Occurs when the selected event changes.
    /// </summary>
    public event EventHandler<TimelineEventView?>? SelectedEventChanged;

    /// <summary>
    /// Gets the current viewport start time.
    /// </summary>
    public DateTime ViewStartTime => _viewStartTime;

    /// <summary>
    /// Gets the current viewport end time.
    /// </summary>
    public DateTime ViewEndTime => _viewEndTime;

    /// <summary>
    /// Gets the current zoom level.
    /// </summary>
    public double ZoomLevel => _zoomLevel;

    /// <summary>
    /// Gets or sets whether to show diagnostics overlay (FPS, draw time, etc.).
    /// </summary>
    public bool ShowDiagnostics
    {
        get => _showDiagnostics;
        set
        {
            if (_showDiagnostics != value)
            {
                _showDiagnostics = value;
                InvalidateCanvas();
            }
        }
    }

    /// <summary>
    /// Gets the current FPS (frames per second).
    /// </summary>
    public double CurrentFps
    {
        get
        {
            if (_frameTimes.Count == 0) return 0;
            var averageFrameTime = _frameTimes.Average();
            return averageFrameTime > 0 ? 1000.0 / averageFrameTime : 0;
        }
    }

    /// <summary>
    /// Gets the average draw time in milliseconds.
    /// </summary>
    public double AverageDrawTimeMs
    {
        get => _frameTimes.Count > 0 ? _frameTimes.Average() : 0;
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        _frameStopwatch.Restart();

        var canvas = e.Surface.Canvas;
        var info = e.Info;

        canvas.Clear(SKColors.White);

        var events = Events;
        if (events == null || events.Count == 0)
        {
            _renderer.DrawEmptyState(canvas, info);
            RecordFrameTime();
            return;
        }

        _renderer.DrawTimeline(canvas, info, events, _viewStartTime, _viewEndTime, _zoomLevel, _showDiagnostics, CurrentFps, AverageDrawTimeMs, GroupByType, SelectedEvent);

        RecordFrameTime();
        EmitTelemetryIfNeeded(events.Count);
    }

    private void RecordFrameTime()
    {
        var frameTime = _frameStopwatch.Elapsed.TotalMilliseconds;
        _frameCount++;

        _frameTimes.Enqueue(frameTime);
        if (_frameTimes.Count > 60)
        {
            _frameTimes.Dequeue();
        }
    }

    private void EmitTelemetryIfNeeded(int eventCount)
    {
        var now = DateTime.UtcNow;
        if ((now - _lastTelemetryEmit).TotalSeconds < TelemetryEmitIntervalSeconds)
            return;

        _lastTelemetryEmit = now;

        // Emit telemetry through logging/telemetry service
        var services = Handler?.MauiContext?.Services;
        var telemetry = services?.GetService<TelemetryService>();
        var logger = services?.GetService<ILogger<SkiaTimelineView>>();

        if (logger != null)
        {
            logger.LogDebug(
                "Timeline performance: FPS={Fps:F1}, DrawTime={DrawTime:F2}ms, Events={EventCount}, Zoom={Zoom:F2}",
                CurrentFps,
                AverageDrawTimeMs,
                eventCount,
                _zoomLevel);
        }

        // Also emit structured telemetry if service available
        if (telemetry != null && telemetry.ConsentGranted)
        {
            // Using ActivitySource pattern similar to other telemetry
            using var activity = new System.Diagnostics.Activity("IdeaBranch.Timeline.Performance");
            activity?.SetTag("fps", CurrentFps);
            activity?.SetTag("draw_time_ms", AverageDrawTimeMs);
            activity?.SetTag("event_count", eventCount);
            activity?.SetTag("zoom_level", _zoomLevel);
        }
    }

    private void OnTouch(object? sender, SKTouchEventArgs e)
    {
        e.Handled = _interactionHandler.HandleTouch(e);
        if (e.Handled)
            InvalidateCanvas();
    }

    /// <summary>
    /// Gets the canvas view for internal access.
    /// </summary>
    internal SKCanvasView? GetCanvasView() => CanvasView;

    /// <summary>
    /// Invokes the SelectedEventChanged event.
    /// </summary>
    internal void InvokeSelectedEventChanged(TimelineEventView? eventView)
    {
        SelectedEventChanged?.Invoke(this, eventView);
    }

    /// <summary>
    /// Invokes the SelectedTimeRangeChanged event.
    /// </summary>
    internal void InvokeSelectedTimeRangeChanged((DateTime Start, DateTime End)? range)
    {
        SelectedTimeRangeChanged?.Invoke(this, range);
    }

    /// <summary>
    /// Invalidates the canvas to trigger a redraw.
    /// </summary>
    public void InvalidateCanvas() => CanvasView.InvalidateSurface();

    /// <summary>
    /// Zooms the timeline by the specified factor.
    /// </summary>
    public void Zoom(double factor, float centerX = 0)
    {
        var oldZoom = _zoomLevel;
        _zoomLevel = Math.Max(0.1, Math.Min(100.0, _zoomLevel * factor));

        if (CanvasView != null && CanvasView.CanvasSize.Width > 0)
        {
            var canvasWidth = CanvasView.CanvasSize.Width;
            var normalizedX = centerX / canvasWidth;
            var timeRange = (_viewEndTime - _viewStartTime).TotalDays;
            var newTimeRange = timeRange / (_zoomLevel / oldZoom);

            var centerTime = _viewStartTime.AddDays(timeRange * normalizedX);
            _viewStartTime = centerTime.AddDays(-newTimeRange * normalizedX);
            _viewEndTime = _viewStartTime.AddDays(newTimeRange);
        }

        InvalidateCanvas();
    }

    /// <summary>
    /// Pans the timeline by the specified offset in pixels.
    /// </summary>
    public void Pan(double offsetPixels)
    {
        if (CanvasView == null || CanvasView.CanvasSize.Width <= 0)
            return;

        var timeRange = (_viewEndTime - _viewStartTime).TotalDays;
        var pixelsPerDay = CanvasView.CanvasSize.Width / timeRange;
        var offsetDays = offsetPixels / pixelsPerDay;

        _viewStartTime = _viewStartTime.AddDays(-offsetDays);
        _viewEndTime = _viewEndTime.AddDays(-offsetDays);

        InvalidateCanvas();
    }

    /// <summary>
    /// Resets the view to show all events.
    /// </summary>
    public void ResetView()
    {
        var events = Events;
        if (events == null || events.Count == 0)
        {
            var now = DateTime.Now;
            _viewStartTime = now.AddYears(-1);
            _viewEndTime = now;
        }
        else
        {
            var earliest = events.Min(e => e.When.Start.Date);
            var latest = events.Max(e => e.When.End?.Date ?? e.When.Start.Date);
            var padding = (latest - earliest).TotalDays * 0.1;
            _viewStartTime = earliest.AddDays(-padding);
            _viewEndTime = latest.AddDays(padding);
        }

        _zoomLevel = 1.0;
        InvalidateCanvas();
    }
}

