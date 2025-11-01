using IdeaBranch.Domain.Timeline;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace IdeaBranch.App.Controls;

/// <summary>
/// Handles touch/mouse interactions for the timeline.
/// </summary>
internal class TimelineInteractionHandler
{
    private readonly SkiaTimelineView _view;
    private readonly TimelineRenderer _renderer;
    private bool _isPanning;
    private bool _isSelecting;
    private SKPoint _lastTouchPoint;
    private SKPoint _selectionStart;

    public TimelineInteractionHandler(SkiaTimelineView view, TimelineRenderer renderer)
    {
        _view = view;
        _renderer = renderer;
    }

    public (DateTime Start, DateTime End)? SelectedRange { get; set; }
    public TimelineEventView? SelectedEvent { get; set; }

    public bool HandleTouch(SKTouchEventArgs e)
    {
        switch (e.ActionType)
        {
            case SKTouchAction.Pressed:
                return HandlePressed(e.Location);

            case SKTouchAction.Moved:
                return HandleMoved(e.Location);

            case SKTouchAction.Released:
                return HandleReleased(e.Location);

            case SKTouchAction.WheelChanged:
                return HandleWheel(e);

            default:
                return false;
        }
    }

    private bool HandlePressed(SKPoint location)
    {
        _lastTouchPoint = location;

        // Check if clicking on an event
        var hitEvent = HitTestEvent(location);
        if (hitEvent != null)
        {
            SelectedEvent = hitEvent;
            _view.InvokeSelectedEventChanged(hitEvent);
            return true;
        }

        // Start selection or pan
        _selectionStart = location;
        _isSelecting = false;
        _isPanning = false;
        return true;
    }

    private bool HandleMoved(SKPoint location)
    {
        var delta = location - _lastTouchPoint;

        if (!_isPanning && !_isSelecting)
        {
            // Determine if this is a pan or selection based on movement
            if (Math.Abs(delta.X) > 5 || Math.Abs(delta.Y) > 5)
            {
                if (Math.Abs(delta.Y) > Math.Abs(delta.X))
                {
                    _isPanning = true;
                }
                else
                {
                    _isSelecting = true;
                }
            }
        }

        if (_isPanning)
        {
            _view.Pan(-delta.X);
            _lastTouchPoint = location;
            return true;
        }

        if (_isSelecting)
        {
            UpdateSelection(_selectionStart, location);
            _lastTouchPoint = location;
            return true;
        }

        return false;
    }

    private bool HandleReleased(SKPoint location)
    {
        if (_isSelecting)
        {
            FinalizeSelection();
        }

        _isPanning = false;
        _isSelecting = false;
        return true;
    }

    private bool HandleWheel(SKTouchEventArgs e)
    {
        // Zoom on wheel
        var zoomFactor = e.WheelDelta > 0 ? 1.1 : 0.9;
        _view.Zoom(zoomFactor, e.Location.X);
        return true;
    }

    private TimelineEventView? HitTestEvent(SKPoint location)
    {
        var events = _view.Events;
        var canvasView = _view.GetCanvasView();
        if (events == null || canvasView == null)
            return null;

        var info = canvasView.CanvasSize;
        var timeRange = (_view.ViewEndTime - _view.ViewStartTime).TotalDays;
        var pixelsPerDay = info.Width / timeRange;

        var hitTime = _view.ViewStartTime.AddDays((location.X / info.Width) * timeRange);

        foreach (var evt in events)
        {
            var eventX = (float)((evt.When.Start.Date - _view.ViewStartTime).TotalDays * pixelsPerDay);
            var eventY = 50.0f;
            var size = 12.0f; // Default size

            var distance = Math.Sqrt(Math.Pow(location.X - eventX, 2) + Math.Pow(location.Y - eventY, 2));
            if (distance <= size)
            {
                return evt;
            }
        }

        return null;
    }

    private void UpdateSelection(SKPoint start, SKPoint end)
    {
        var canvasView = _view.GetCanvasView();
        if (canvasView == null) return;

        var info = canvasView.CanvasSize;
        var timeRange = (_view.ViewEndTime - _view.ViewStartTime).TotalDays;

        var startTime = _view.ViewStartTime.AddDays((start.X / info.Width) * timeRange);
        var endTime = _view.ViewStartTime.AddDays((end.X / info.Width) * timeRange);

        if (startTime > endTime)
        {
            (startTime, endTime) = (endTime, startTime);
        }

        SelectedRange = (startTime, endTime);
    }

    private void FinalizeSelection()
    {
        if (SelectedRange.HasValue)
        {
            _view.InvokeSelectedTimeRangeChanged(SelectedRange);
        }
    }
}

