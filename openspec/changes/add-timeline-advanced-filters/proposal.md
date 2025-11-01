## Why
The current Timeline Analytics filtering is limited to basic date range and source type selection. Users need more powerful query capabilities to find specific events across large datasets, including hierarchical tag filtering, event type filtering (Created/Updated), quick date presets, free-text search, and the ability to combine multiple filter criteria with defined boolean logic.

## What Changes
- Add hierarchical tag picker with per-tag "Include descendants" toggle (default OFF)
- Add event type filtering (Created/Updated) with both types active by default
- Add quick date presets: Last 7 days, This month, This year
- Add free-text search input that searches within event titles, bodies, tag names, source/service names, and actor display names
- Implement faceted boolean logic: AND across different facets, OR within a facet (e.g., Tags facet uses OR internally)

## Impact
- Affected specs: `timeline-analytics` (new capability)
- Affected code: `IdeaBranch.App/Views/TimelinePage.xaml`, `IdeaBranch.App/ViewModels/Analytics/TimelineViewModel.cs`, filtering query logic in timeline service layer

