## Why
The current Timeline view displays temporal annotations as a static vertical list of bands, which limits exploration of dense datasets and provides poor performance with large numbers of events. Users need an interactive, high-performance timeline visualization that supports zooming, scrolling, filtering, and exploration of events across different time scales.

## What Changes
- Replace static list-based timeline with interactive SkiaSharp-based canvas renderer
- Add zoom/pan support with precision-aware rendering (day/week/month/year scales)
- Add event markers sized and colored by type with legend
- Add click/tap interaction for event details
- Add drag-to-select range filtering
- Add event clustering for dense periods with progressive expansion on zoom
- Add performance optimizations (culling, virtualization) to handle 50k+ events smoothly

## Impact
- Affected specs: `ui`, `analytics`
- Affected code: `IdeaBranch.App/Views/TimelinePage.xaml`, `IdeaBranch.App/ViewModels/Analytics/TimelineViewModel.cs`, new `IdeaBranch.App/Controls/SkiaTimelineView.cs`, new `IdeaBranch.Domain/Timeline/` models

